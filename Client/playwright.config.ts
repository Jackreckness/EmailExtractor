import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  workers: 1,
  retries: 0,
  outputDir: './test-results',
  use: {
    baseURL: 'http://127.0.0.1:17778',
    browserName: 'chromium',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command: 'dotnet run --project ../APIService/src/ApiService.csproj --no-launch-profile -- --urls http://127.0.0.1:17777',
      // The API has no GET health endpoint; wait for its listening socket.
      port: 17777,
      reuseExistingServer: false,
      timeout: 120_000,
    },
    {
      command: 'npm run dev -- --config vite.e2e.config.ts',
      url: 'http://127.0.0.1:17778',
      reuseExistingServer: false,
      timeout: 60_000,
    },
  ],
})
