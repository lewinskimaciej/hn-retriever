import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '60s', target: 10000 },
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
