"""Small Spanish semantic regression set; synthetic inputs only."""
import json, pathlib, requests, io, soundfile as sf, numpy as np
root=pathlib.Path(__file__).resolve().parents[3]
headers={'X-BB8-Token':(root/'.local-voice/token').read_text().strip()}
cases=[('Estoy feliz de verte','happy','celebrate'),('Me siento triste y solo','sad','comfort'),('Aléjate de mí, robot inútil','angry','retreat'),('Tengo miedo, quédate conmigo','fear','comfort'),('No estoy triste, estoy tranquilo','neutral','attentive'),('Me da rabia que cancelaran mi viaje','angry','attentive'),('Ayer estaba triste pero ahora estoy feliz','happy','celebrate'),('¿Qué hora es?','neutral','attentive')]
results=[]
for text,emotion,reaction in cases:
 r=requests.post('http://127.0.0.1:8768/classify',headers=headers,json={'text':text,'history':[]},timeout=90);r.raise_for_status();d=r.json()
 results.append({'input':text,'pass':d['emotion']==emotion and d['reaction']==reaction,'result':d})
b=io.BytesIO();sf.write(b,np.zeros(16000),16000,format='WAV')
r=requests.post('http://127.0.0.1:8768/voice',headers=headers,data=b.getvalue(),timeout=90)
results.append({'input':'silence','pass':r.status_code==422})
r=requests.post('http://127.0.0.1:8768/classify',json={'text':'hola'},timeout=5)
results.append({'input':'missing local token','pass':r.status_code==403})
(root/'Logs').mkdir(exist_ok=True)
(root/'Logs/voice-verification.json').write_text(json.dumps(results,ensure_ascii=False,indent=2))
for d in results: print('PASS' if d['pass'] else 'FAIL',d['input'])
raise SystemExit(0 if all(d['pass'] for d in results) else 1)
