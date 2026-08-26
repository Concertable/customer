import path from "path"
import tailwindcss from "@tailwindcss/vite"
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { tanstackRouter } from '@tanstack/router-vite-plugin'
import { aspNetDevelopmentHttps } from '../../scripts/vite-development-https'

export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, path.resolve(__dirname, '../'), 'VITE_')
  return {
    plugins: [tanstackRouter(), react(), tailwindcss()],
    server: {
      host: '127.0.0.1',
      https: command === 'serve'
        ? aspNetDevelopmentHttps(path.resolve(__dirname, '../../node_modules/.vite/aspnet-https/customer'))
        : undefined,
      port: 5174,
    },
    envDir: '../',
    define: command === 'build'
      ? {
          'import.meta.env.VITE_OIDC_CLIENT_ID': JSON.stringify('customer-web'),
          'import.meta.env.VITE_OIDC_SCOPE': JSON.stringify('openid profile roles concertable.customer.api concertable.search.api offline_access'),
          'import.meta.env.VITE_API_URL': JSON.stringify(env.VITE_CUSTOMER_API_URL),
          'import.meta.env.VITE_BASE_URL': JSON.stringify(env.VITE_CUSTOMER_API_URL.replace(/\/api\/?$/, '')),
        }
      : {
          // dev/E2E: standalone Customer AppHost pins auth→7093 / search→7097 (its Program.cs), NOT the
          // umbrella host's .env.development 7083/7087 — don't "reconcile" these or dev auth/search breaks.
          'import.meta.env.VITE_AUTH_AUTHORITY': JSON.stringify('https://localhost:7093'),
          'import.meta.env.VITE_OIDC_CLIENT_ID': JSON.stringify('customer-web'),
          'import.meta.env.VITE_OIDC_SCOPE': JSON.stringify('openid profile roles concertable.customer.api concertable.search.api offline_access'),
          'import.meta.env.VITE_API_URL': JSON.stringify('https://localhost:7090/api'),
          'import.meta.env.VITE_BASE_URL': JSON.stringify('https://localhost:7090'),
          'import.meta.env.VITE_SEARCH_API_URL': JSON.stringify('https://localhost:7097/api'),
          'import.meta.env.VITE_PAYMENT_API_URL': JSON.stringify('https://localhost:7088/api'),
        },
    resolve: {
      alias: [
        { find: "@", replacement: path.resolve(__dirname, "./src") },
      ],
    },
  }
})
