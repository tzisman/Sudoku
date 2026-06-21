import { chromium } from 'playwright';
import { mkdir } from 'fs/promises';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const outDir = path.join(__dirname, 'screenshots');
const baseUrl = process.env.SUDOKU_URL || 'http://localhost:5177';

await mkdir(outDir, { recursive: true });

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });

async function openPanel() {
  await page.evaluate(() => {
    document.body.classList.add('panel-open');
    document.getElementById('panel-toggle')?.setAttribute('aria-expanded', 'true');
    document.getElementById('panel-backdrop')?.classList.remove('hidden');
  });
}

async function closePanel() {
  await page.evaluate(() => {
    document.body.classList.remove('panel-open');
    document.getElementById('panel-toggle')?.setAttribute('aria-expanded', 'false');
    document.getElementById('panel-backdrop')?.classList.add('hidden');
  });
}

async function click(id) {
  await page.evaluate((selector) => document.querySelector(selector)?.click(), id);
}

async function waitForIdle() {
  await page.waitForFunction(() => document.getElementById('loading')?.classList.contains('hidden'));
}

await page.goto(baseUrl, { waitUntil: 'networkidle' });
await page.screenshot({ path: path.join(outDir, '01-home.png'), fullPage: true });

await openPanel();
await page.waitForTimeout(300);
await page.screenshot({ path: path.join(outDir, '02-menu.png'), fullPage: true });

await click('#btn-new');
await waitForIdle();
await page.waitForTimeout(400);
await closePanel();
await page.screenshot({ path: path.join(outDir, '03-new-puzzle.png'), fullPage: true });

await openPanel();
await page.waitForTimeout(300);
await page.screenshot({ path: path.join(outDir, '04-menu-with-puzzle.png'), fullPage: true });
await closePanel();

await click('#btn-solve');
await waitForIdle();
await page.waitForFunction(() => !document.getElementById('solution-section')?.classList.contains('hidden'));
await page.waitForTimeout(400);
await page.screenshot({ path: path.join(outDir, '05-solution.png'), fullPage: true });

await page.goto(`${baseUrl}/swagger`, { waitUntil: 'networkidle' });
await page.waitForTimeout(500);
await page.screenshot({ path: path.join(outDir, '06-swagger.png'), fullPage: true });

await browser.close();
console.log('Screenshots saved to', outDir);
