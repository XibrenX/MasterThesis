import requests
from stem import Signal
from stem.control import Controller
import logging
import time
import random

logger = logging.getLogger('tor')

class HeaderSetter:
    def set_session_headers(session: requests.Session):
        session.headers = {}
        session.headers['User-Agent'] = 'Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0'
        session.headers['Accept'] = 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8'
        session.headers['Accept-Language'] = 'en-US,en;q=0.5'
        session.headers['Connection'] = 'keep-alive'
        session.headers['Dnt'] = '1'
        session.headers['Upgrade-Insecure-Requests'] = '1'
        session.headers['Sec-Fetch-Dest'] = 'document'
        session.headers['Sec-Fetch-Mode'] = 'navigate'
        session.headers['Sec-Fetch-Site'] = 'none'
        session.headers['Sec-Fetch-User'] = '?1'

class Tor:
    def __init__(self, tor_password: str, tor_controller_port:int = 9051, tor_proxy_port:int = 9050):
        self.tor_password = tor_password
        self.tor_controller_port = tor_controller_port
        self.tor_proxy_port=tor_proxy_port
        self.refresh_session()

    def get_ip(self) -> str:
        try:
            return self.session.get('https://httpbin.org/ip').json()['origin']
        except Exception as e:
            logger.exception('Could not get ip')
            print(str(e))

    def refresh_session(self):
        with Controller.from_port(port = self.tor_controller_port) as controller:
            controller.authenticate(password=self.tor_password)
            controller.signal(Signal.NEWNYM)
        
        self.session = requests.session()
        self.session.proxies = {}
        self.session.proxies['http']= f'socks5h://localhost:{self.tor_proxy_port}'
        self.session.proxies['https']= f'socks5h://localhost:{self.tor_proxy_port}'
        HeaderSetter.set_session_headers(self.session)

    def get_session(self) -> requests.Session:
        return self.session

class WithoutTor:
    def __init__(self):
        self.refresh_session()

    def refresh_session(self):
        self.session = requests.session()
        HeaderSetter.set_session_headers(self.session)

    def get_session(self) -> requests.Session:
        return self.session

class SessionLimiter:
    def __init__(self, other, min_refresh_interval: int = 10, max_refresh_interval: int = 13):
        self.other = other
        self.min_refresh_interval = min_refresh_interval
        self.max_refresh_interval = max_refresh_interval
        self.session_left = random.randint(self.min_refresh_interval, self.max_refresh_interval)

    def get_session(self) -> requests.Session:
        self.session_left -= 1
        if self.session_left < 0:
            logger.info('Session limit reaced, retrieving new ip')
            self.other.refresh_session()
            self.session_left = random.randint(self.min_refresh_interval, self.max_refresh_interval)

        return self.other.get_session()

class SessionDelayLimiter:
    def __init__(self, other, min_delay_sec: int = 12, max_delay_sec: int = 22):
        self.other = other
        self.min_delay_sec = min_delay_sec
        self.max_delay_sec = max_delay_sec

    def get_session(self):
        return self.other.get_session()
    
    def refresh_session(self):
        time.sleep(random.randint(self.min_delay_sec, self.max_delay_sec))
        self.other.refresh_session()


class DelayLimiter:
    def __init__(self, other, min_delay_sec: int = 7, max_delay_sec: int = 12):
        self.other = other
        self.min_delay_sec = min_delay_sec
        self.max_delay_sec = max_delay_sec
        self.next_allowed_time = 0

    def get_session(self) -> requests.Session:
        if time.time() < self.next_allowed_time:
            time.sleep(self.next_allowed_time - time.time())
        
        self.next_allowed_time = time.time() + random.randint(self.min_delay_sec, self.max_delay_sec)

        return self.other.get_session()

class TorIpReuseLimiter:
    def __init__(self, tor: Tor, min_ip_reuse_interval: int = 10):
        self.tor = tor
        self.min_ip_reuse_interval = min_ip_reuse_interval

        self.ip_history = []
        ip = self.tor.get_ip()
        logger.info(f'Now running with ip: {ip}')
        self.ip_history.append(ip)
        self.log_headers()

    def get_session(self) -> requests.Session:
        return self.tor.get_session()
    
    def log_headers(self):
        logger.info('Current headers: ' + self.tor.get_session().get('https://httpbin.org/headers').text)

    def refresh_session(self):
        while True:
            self.tor.refresh_session()
            ip = self.tor.get_ip()
            if ip not in self.ip_history:
                break
        self.ip_history.append(ip)
        logger.info(f'Now running with ip {ip}')
        self.get_session_left = self.ip_refresh_interval
        self.log_headers()

        while len(self.ip_history) > self.min_ip_reuse_interval:
            self.ip_history.pop(0)
        
