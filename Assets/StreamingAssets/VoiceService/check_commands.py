"""Check deterministic command routing without loading speech or language models."""
import ast
from pathlib import Path
import re
import unicodedata

source = Path(__file__).with_name('service.py')
tree = ast.parse(source.read_text())
functions = [node for node in tree.body if isinstance(node, ast.FunctionDef)
             and node.name in {'special_command', 'classify'}]
namespace = {'re': re, 'unicodedata': unicodedata}
exec(compile(ast.Module(body=functions, type_ignores=[]), str(source), 'exec'), namespace)
cases = {'sígueme': 'follow', 'BB-8, sígueme por favor': 'follow',
         'sigueme': 'follow', 'aléjate': 'away', 'alejate de mi por favor': 'away',
         'no te alejes': None, 'no me sigas': None,
         'quiero que digas sigueme': None, 'estoy feliz': None,
         '"sígueme"': None, 'si te digo aléjate': None}
for phrase, expected in cases.items():
    assert namespace['special_command'](phrase) == expected, phrase
    if expected:
        result = namespace['classify'](phrase, ['estoy furioso contigo'])
        assert result['command'] == expected and result['attitude'] == 'neutral'
print(f'PASS {len(cases)} command cases; direct orders bypass model and history')
