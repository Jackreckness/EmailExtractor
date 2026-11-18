import { defineConfig, mergeConfig } from 'vite'
import config from './vite.config.ts'

// Dedicated smoke-test ports leave the normal development servers untouched.
export default mergeConfig(config, defineConfig({
  server: {
    port: 17778,
    proxy: { '/api': 'http://127.0.0.1:17777' },
  },
}))
