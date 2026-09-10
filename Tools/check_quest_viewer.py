"""Isolated localhost transport checks; no headset, real credential or game frame."""
import json
import threading
import time
import unittest
from unittest.mock import patch
import urllib.error
import urllib.request

with patch('pathlib.Path.read_text',return_value='test-only-key'):
    import quest_viewer as viewer


class ViewerChecks(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.server=viewer.ThreadingHTTPServer(('127.0.0.1',0),viewer.Handler)
        cls.base='http://127.0.0.1:'+str(cls.server.server_port)
        cls.thread=threading.Thread(target=cls.server.serve_forever,daemon=True);cls.thread.start()
    @classmethod
    def tearDownClass(cls):cls.server.shutdown();cls.server.server_close();cls.thread.join()
    def setUp(self):
        with viewer.lock:viewer.frame=b'';viewer.frame_time=0;viewer.viewer_time=0
    def request(self,path,data=None,token=None):
        headers={} if token is None else {'X-BB8-Token':token}
        req=urllib.request.Request(self.base+path,data=data,headers=headers)
        try:return urllib.request.urlopen(req,timeout=2)
        except urllib.error.HTTPError as response:return response
    def test_viewer_presence_expires(self):
        with self.request('/status') as r:self.assertFalse(json.load(r)['active'])
        with self.request('/frame') as r:self.assertEqual(r.status,204)
        with self.request('/status') as r:self.assertTrue(json.load(r)['active'])
        with viewer.lock:viewer.viewer_time=time.monotonic()-4
        with self.request('/status') as r:self.assertFalse(json.load(r)['active'])
    def test_upload_requires_token(self):
        with self.request('/frame',b'bad') as r:self.assertEqual(r.status,403)
    def test_invalid_frame_rejected(self):
        with self.request('/frame',b'bad','test-only-key') as r:self.assertEqual(r.status,415)
    def test_latest_frame_only_and_no_cache(self):
        for payload in (b'\xff\xd8first\xff\xd9',b'\xff\xd8latest\xff\xd9'):
            with self.request('/frame',payload,'test-only-key') as r:self.assertEqual(r.status,204)
        with self.request('/frame') as r:
            self.assertEqual(r.read(),payload);self.assertEqual(r.headers['Cache-Control'],'no-store')
            self.assertEqual(r.headers['Cross-Origin-Resource-Policy'],'same-origin')


if __name__=='__main__':unittest.main()
