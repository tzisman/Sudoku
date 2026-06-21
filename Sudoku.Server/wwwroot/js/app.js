const puzzleGrid = document.getElementById('puzzle-grid');
const solutionGrid = document.getElementById('solution-grid');
const solutionSection = document.getElementById('solution-section');
const statusEl = document.getElementById('status');
const loadingEl = document.getElementById('loading');
const loadingText = document.getElementById('loading-text');
const difficultySelect = document.getElementById('difficulty');
const bookletCountSelect = document.getElementById('booklet-count');

const actionButtons = document.querySelectorAll('.panel-btn');
const panelToggle = document.getElementById('panel-toggle');
const panelBackdrop = document.getElementById('panel-backdrop');

const API_TIMEOUT_MS = 90000;
const FIX_TIMEOUT_MS = 180000;

function createEmptyGrid() {
    return Array.from({ length: 9 }, () => Array(9).fill(0));
}

function showSolution(grid) {
    renderGrid(solutionGrid, grid, false);
    solutionSection.classList.remove('hidden');
    requestAnimationFrame(() => {
        solutionSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    });
}

function renderGrid(container, grid, editable = false, prefilled = null) {
    container.innerHTML = '';
    container.className = editable ? 'sudoku-grid' : 'sudoku-grid sudoku-grid--readonly';

    for (let row = 0; row < 9; row++) {
        for (let col = 0; col < 9; col++) {
            const cell = document.createElement(editable ? 'input' : 'div');
            cell.className = 'cell';
            if (col === 2 || col === 5) cell.classList.add('thick-right');
            if (row === 2 || row === 5) cell.classList.add('thick-bottom');

            const boxIndex = Math.floor(row / 3) * 3 + Math.floor(col / 3);
            if (boxIndex % 2 === 1) cell.classList.add('box-alt');

            const value = grid[row][col];
            const isFixed = prefilled && prefilled[row][col] !== 0;

            if (editable) {
                cell.type = 'text';
                cell.maxLength = 1;
                cell.inputMode = 'numeric';
                cell.dataset.row = row;
                cell.dataset.col = col;
                cell.value = value === 0 ? '' : value.toString();
                if (isFixed) {
                    cell.readOnly = true;
                    cell.classList.add('prefilled');
                    cell.tabIndex = -1;
                } else {
                    cell.addEventListener('input', onCellInput);
                }
            } else {
                cell.textContent = value === 0 ? '' : value.toString();
            }

            container.appendChild(cell);
        }
    }
}

function onCellInput(e) {
    const input = e.target;
    input.value = input.value.replace(/[^1-9]/g, '').slice(-1);
}

function readGridFromDom() {
    const grid = createEmptyGrid();
    puzzleGrid.querySelectorAll('input').forEach(input => {
        const row = parseInt(input.dataset.row, 10);
        const col = parseInt(input.dataset.col, 10);
        grid[row][col] = input.value === '' ? 0 : parseInt(input.value, 10);
    });
    return grid;
}

function setStatus(message, isError = false) {
    statusEl.textContent = message;
    statusEl.classList.toggle('error', isError);
}

function setLoading(active, text = 'working...') {
    loadingEl.classList.toggle('hidden', !active);
    loadingText.textContent = text;
    actionButtons.forEach(btn => { btn.disabled = active; });
}

async function apiPost(url, body, timeoutMs = API_TIMEOUT_MS) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: body instanceof FormData ? {} : { 'Content-Type': 'application/json' },
            body: body instanceof FormData ? body : JSON.stringify(body),
            signal: controller.signal
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || response.statusText);
        }

        return response;
    } catch (err) {
        if (err.name === 'AbortError') {
            throw new Error('Request timed out. Please try again.');
        }
        throw err;
    } finally {
        clearTimeout(timer);
    }
}

async function handleNew() {
    setLoading(true, 'creating puzzle...');
    setStatus('');
    solutionSection.classList.add('hidden');
    currentPrefilled = null;

    try {
        const response = await apiPost('/api/sudoku/new', {
            difficulty: difficultySelect.value === 'normal' ? null : difficultySelect.value
        });
        const data = await response.json();
        currentPrefilled = data.puzzle.map(row => [...row]);
        renderGrid(puzzleGrid, data.puzzle, true, currentPrefilled);
        setStatus('new puzzle ready');
    } catch (err) {
        setStatus(err.message, true);
    } finally {
        setLoading(false);
    }
}

let currentPrefilled = null;

function handleBlank() {
    solutionSection.classList.add('hidden');
    currentPrefilled = null;
    renderGrid(puzzleGrid, createEmptyGrid(), true);
    setStatus('blank board — fill in the cells');
}

async function handleSolve() {
    setLoading(true, 'solving...');
    setStatus('');

    try {
        const grid = readGridFromDom();
        const response = await apiPost('/api/sudoku/solve', { grid });
        const data = await response.json();

        if (data.solved && data.solution) {
            showSolution(data.solution);
        } else {
            solutionSection.classList.add('hidden');
        }

        setStatus(data.solved ? 'solved!' : 'could not fully solve this puzzle', !data.solved);
    } catch (err) {
        setStatus(err.message, true);
    } finally {
        setLoading(false);
    }
}

function countClues(grid) {
    return grid.flat().filter(v => v !== 0).length;
}

async function handleFix() {
    setLoading(true, 'removing extras...');
    setStatus('');
    solutionSection.classList.add('hidden');

    const grid = readGridFromDom();
    if (countClues(grid) === 0) {
        setStatus('fill in the board first', true);
        setLoading(false);
        return;
    }

    try {
        const response = await apiPost('/api/sudoku/fix', { grid }, FIX_TIMEOUT_MS);
        const data = await response.json();

        currentPrefilled = data.puzzle.map(row => [...row]);
        renderGrid(puzzleGrid, data.puzzle, true, currentPrefilled);

        const before = data.cluesBefore ?? countClues(grid);
        const after = data.cluesAfter ?? countClues(data.puzzle);

        if (after < before) {
            setStatus(`extras removed (${before} → ${after} clues)`);
        } else {
            setStatus('puzzle is already minimal');
        }
    } catch (err) {
        const msg = err.message || '';
        if (msg.includes('405') || msg.includes('404')) {
            setStatus('server is outdated — restart the app and try again', true);
        } else {
            setStatus(msg, true);
        }
    } finally {
        setLoading(false);
    }
}

async function handlePdf() {
    setLoading(true, 'generating pdf...');
    setStatus('');

    try {
        const grid = readGridFromDom();
        const response = await apiPost('/api/sudoku/pdf', { grid, title: 'MySudoku' }, 120000);
        await downloadBlob(response, 'sudoku.pdf');
        setStatus('pdf downloaded');
    } catch (err) {
        setStatus(err.message, true);
    } finally {
        setLoading(false);
    }
}

async function handleBooklet() {
    const count = parseInt(bookletCountSelect.value, 10);
    setLoading(true, `generating booklet (${count} pages)...`);
    setStatus('');

    try {
        const difficulty = difficultySelect.value === 'normal' ? null : difficultySelect.value;
        const response = await apiPost('/api/sudoku/booklet', { count, difficulty }, count * 120000);
        await downloadBlob(response, 'sudoku-booklet.pdf');
        setStatus(`booklet (${count} pages) downloaded`);
    } catch (err) {
        setStatus(err.message, true);
    } finally {
        setLoading(false);
    }
}

async function downloadBlob(response, filename) {
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
}

document.getElementById('btn-new').addEventListener('click', handleNew);
document.getElementById('btn-blank').addEventListener('click', handleBlank);
document.getElementById('btn-solve').addEventListener('click', handleSolve);
document.getElementById('btn-fix').addEventListener('click', handleFix);
document.getElementById('btn-pdf').addEventListener('click', handlePdf);
document.getElementById('btn-booklet').addEventListener('click', handleBooklet);

function setPanelOpen(open) {
    document.body.classList.toggle('panel-open', open);
    panelToggle.setAttribute('aria-expanded', open);
    panelToggle.setAttribute('aria-label', open ? 'Close menu' : 'Open menu');
    panelBackdrop.classList.toggle('hidden', !open);
    try { localStorage.setItem('panelOpen', open ? '1' : '0'); } catch (_) {}
}

panelToggle.addEventListener('click', () => {
    setPanelOpen(!document.body.classList.contains('panel-open'));
});

panelBackdrop.addEventListener('click', () => setPanelOpen(false));

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && document.body.classList.contains('panel-open')) {
        setPanelOpen(false);
    }
});

try {
    if (localStorage.getItem('panelOpen') === '1') setPanelOpen(true);
} catch (_) {}

renderGrid(puzzleGrid, createEmptyGrid(), true);
setStatus('click blank board or new to start');
