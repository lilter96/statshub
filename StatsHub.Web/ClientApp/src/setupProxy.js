const {createProxyMiddleware} = require('http-proxy-middleware');
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
    : env.ASPNETCORE_URLS
        ? env.ASPNETCORE_URLS.split(';')[0]
        : 'https://localhost:7065';

module.exports = function (app) {
    const apiProxyOptions = {
        target: target,
        changeOrigin: true,
        secure: false,
        onError: (err, req, res) => {
            console.error(`Proxy error [${req.url}]: ${err.message}`);
            if (!res.headersSent) {
                res.writeHead(500, {'Content-Type': 'text/plain'});
            }
            res.end('Proxy error.');
        }
    };

    app.use(
        ['/orders', '/negotiate'],
        createProxyMiddleware(apiProxyOptions)
    );
};
