import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 5174, // Aspire targetPort — do NOT change (5173 is Aspire's public proxy port)
        strictPort: true,
        host: true,
        proxy: {
            // Same-origin proxy: the browser only talks to the Aspire endpoint (localhost:5173),
            // so the auth cookies (fp, refresh_token) become first-party and SameSite=Lax works.
            // Target the API over HTTP so the cookies aren't flagged Secure (an http page can't
            // store Secure cookies). Requires HTTPS redirection to be off in Development.
            '/v1': {
                target: 'http://localhost:5270',
                changeOrigin: true,
            },
        }
    }
})
