import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '15s', target: 100 },
        { duration: '15s', target: 200 },
        { duration: '15s', target: 300 },
        { duration: '15s', target: 400 },
        { duration: '15s', target: 500 },
    ],
};

export default function () {
    const count = Math.floor(Math.random() * 500) + 1;
    const res = http.get(`http://localhost:5107/best-stories?topCount=${count}`);
    check(res, {
        'status is 200': (r) => r.status === 200,
        'body is not empty': (r) => r.body && r.body.length > 0,
    });
    sleep(0.1);
}
