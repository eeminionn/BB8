"""Local USB viewer. Keeps only the latest game frame in memory; never records audio."""
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json
import secrets
import threading
import time

TOKEN=(Path.home()/'Library/Application Support/BB8Voice/.local-voice/token').read_text().strip()
PAGE=Path(__file__).with_suffix('.html').read_bytes()
frame=b''
frame_time=0.
viewer_time=0.
lock=threading.Lock()


class Handler(BaseHTTPRequestHandler):
    def log_message(self,*args):pass
    def send(self,code,body=b'',kind='application/json'):
        self.send_response(code);self.send_header('Content-Type',kind)
        self.send_header('Content-Length',str(len(body)));self.send_header('Cache-Control','no-store')
        self.send_header('Cross-Origin-Resource-Policy','same-origin');self.send_header('X-Frame-Options','DENY')
        self.end_headers()
        if body:self.wfile.write(body)
    def do_GET(self):
        global viewer_time
        if self.path=='/':return self.send(200,PAGE,'text/html; charset=utf-8')
        if self.path=='/health':return self.send(200,b'{"service":"bb8-quest-viewer"}')
        if self.path=='/status':
            with lock:active=viewer_time>0 and time.monotonic()-viewer_time<3
            return self.send(200,json.dumps({'active':active}).encode())
        if self.path=='/frame':
            with lock:
                viewer_time=time.monotonic();image=frame if viewer_time-frame_time<3 else b''
            return self.send(200 if image else 204,image,'image/jpeg')
        self.send(404)
    def do_POST(self):
        global frame,frame_time
        if self.path!='/frame':return self.send(404)
        if not secrets.compare_digest(self.headers.get('X-BB8-Token',''),TOKEN):return self.send(403)
        try:length=int(self.headers.get('Content-Length','0'))
        except ValueError:return self.send(400)
        if not 0<length<=500_000:return self.send(413)
        image=self.rfile.read(length)
        if not image.startswith(b'\xff\xd8') or not image.endswith(b'\xff\xd9'):return self.send(415)
        with lock:frame=image;frame_time=time.monotonic()
        self.send(204)


if __name__=='__main__':ThreadingHTTPServer(('127.0.0.1',8769),Handler).serve_forever()
