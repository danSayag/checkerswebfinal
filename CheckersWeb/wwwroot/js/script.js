let connection = null;

async function startSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/gamehub")
        .build();

    connection.on("LoadBoard", (savedBoard, turn) => {
        boardState = normalizeBoard(savedBoard);
        currentTurn = turn;
        updateStatus();
        renderBoard();
    });

    connection.on("PlayerJoined", (username) => {
        const statusEl = document.getElementById("status");
        if (statusEl) statusEl.textContent = username + " joined the game.";
    });

    connection.on("MoveMade", (move) => {
        boardState = normalizeBoard(move.board);
        currentTurn = move.nextTurn;
        if (move.winner) {
            showWinner(move.winner);
        } else {
            updateStatus();
        }
        selectedCell = null;
        renderBoard();
    });

    try {
        await connection.start();
        await connection.invoke("JoinGame", window.gameId);
    } catch (err) {
        console.error(err);
    }
}

let boardState = Array(8).fill(null).map(() => Array(8).fill(null));
let selectedCell = null;
let currentTurn = "white";

function updateStatus() {
    const el = document.getElementById("status");
    if (!el) return;
    const isMyTurn = currentTurn === window.myColor;
    el.textContent = isMyTurn
        ? "Your turn (" + currentTurn + ")"
        : "Waiting for opponent... (" + currentTurn + "'s turn)";
    el.style.color = isMyTurn ? "green" : "#888";
}

function showWinner(username) {
    const banner = document.getElementById("winner-banner");
    const status = document.getElementById("status");
    if (banner) {
        banner.textContent = "🏆 " + username + " wins!";
        banner.classList.remove("d-none");
    }
    if (status) status.textContent = "";
}

function initBoard() {
    const board = document.getElementById("board");
    if (!board) return;
    board.innerHTML = "";
    for (let row = 0; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
            const divCell = document.createElement("div");
            divCell.dataset.row = row;
            divCell.dataset.col = col;
            divCell.classList.add("game-cell");
            divCell.classList.add((row + col) % 2 === 0 ? "blackDiv" : "whiteDiv");
            board.appendChild(divCell);
        }
    }
    board.addEventListener("click", handleCellClick);
    renderBoard();
}

function renderBoard() {
    const board = document.getElementById("board");
    if (!board) return;
    const cells = board.getElementsByClassName("game-cell");

    for (let i = 0; i < cells.length; i++) {
        const cell = cells[i];
        const row = +cell.dataset.row;
        const col = +cell.dataset.col;

        cell.innerHTML = "";
        cell.classList.remove("selected", "valid-move");

        const piece = boardState[row] && boardState[row][col];
        if (piece) {
            const img = document.createElement("img");
            img.classList.add("piece");
            const file = piece.color === "black"
                ? (piece.isKing ? "blackKing.png" : "blackCell.png")
                : (piece.isKing ? "whiteKing.png" : "whiteCell.png");
            img.src = "/images/" + file;
            img.alt = piece.color + (piece.isKing ? " king" : " piece");
            cell.appendChild(img);
        }

        if (selectedCell && selectedCell.row === row && selectedCell.col === col) {
            cell.classList.add("selected");
        }
    }

    if (selectedCell) {
        highlightValidMoves(selectedCell.row, selectedCell.col);
    }
}

function highlightValidMoves(fromR, fromC) {
    const piece = getPiece(fromR, fromC);
    if (!piece) return;

    const color = piece.color;
    const isKing = piece.isKing;
    const board = document.getElementById("board");
    if (!board) return;

    const candidates = [];

    if (isKing) {
        const dirs = [[-1, -1], [-1, 1], [1, -1], [1, 1]];
        for (const [dr, dc] of dirs) {
            for (let dist = 1; dist <= 7; dist++) {
                const r = fromR + dr * dist, c = fromC + dc * dist;
                if (r < 0 || r > 7 || c < 0 || c > 7) break;
                const sq = getPiece(r, c);
                if (!sq) {
                    candidates.push([r, c]);
                } else {
                    if (sq.color !== color) {
                        const landR = r + dr, landC = c + dc;
                        if (landR >= 0 && landR <= 7 && landC >= 0 && landC <= 7 && !getPiece(landR, landC))
                            candidates.push([landR, landC]);
                    }
                    break;
                }
            }
        }
    } else {
        const fwd = color === "white" ? -1 : 1;
        for (const dc of [-1, 1]) {
            const r = fromR + fwd, c = fromC + dc;
            if (r >= 0 && r <= 7 && c >= 0 && c <= 7 && !getPiece(r, c))
                candidates.push([r, c]);
        }
        for (const dc of [-1, 1]) {
            const midR = fromR + fwd, midC = fromC + dc;
            const landR = fromR + 2 * fwd, landC = fromC + 2 * dc;
            if (landR >= 0 && landR <= 7 && landC >= 0 && landC <= 7) {
                const mid = getPiece(midR, midC);
                if (mid && mid.color !== color && !getPiece(landR, landC))
                    candidates.push([landR, landC]);
            }
        }
    }

    for (const [r, c] of candidates) {
        const cell = board.querySelector(`[data-row="${r}"][data-col="${c}"]`);
        if (cell) cell.classList.add("valid-move");
    }
}

function normalizePiece(p) {
    if (!p) return null;
    return {
        color: p.color || p.Color || "",
        isKing: p.isKing || p.IsKing || false
    };
}

function normalizeBoard(board) {
    return board.map(row => row.map(cell => cell ? normalizePiece(cell) : null));
}

function getPiece(r, c) {
    return boardState[r] ? boardState[r][c] : null;
}

async function handleCellClick(e) {
    const cell = e.target.closest(".game-cell");
    if (!cell) return;

    const row = +cell.dataset.row;
    const col = +cell.dataset.col;

    if (currentTurn !== window.myColor) return;

    if (!selectedCell) {
        const piece = getPiece(row, col);
        const color = piece ? piece.color : "";
        if (piece && color === currentTurn) {
            selectedCell = { row, col };
            renderBoard();
        }
        return;
    }

    if (selectedCell.row === row && selectedCell.col === col) {
        selectedCell = null;
        renderBoard();
        return;
    }

    const clickedPiece = getPiece(row, col);
    if (clickedPiece) {
        const clickedColor = clickedPiece.color;
        if (clickedColor === currentTurn) {
            selectedCell = { row, col };
            renderBoard();
            return;
        }
    }

    try {
        await connection.invoke("MakeMove", window.gameId, selectedCell.row, selectedCell.col, row, col);
    } catch (err) {
        const msg = err.message || String(err);
        const clean = msg.replace(/^HubException: /, "").replace(/^Error: /, "");
        showError(clean);
    }

    selectedCell = null;
    renderBoard();
}


function showWinner(username) {
    const banner = document.getElementById("winner-banner");
    const text = document.getElementById("winner-text");
    const status = document.getElementById("status");
    if (text) text.textContent = username + " wins!";
    if (banner) banner.classList.remove("d-none");
    if (status) status.textContent = "";
}



function showError(msg) {
    let toast = document.getElementById("error-toast");
    if (!toast) {
        toast = document.createElement("div");
        toast.id = "error-toast";
        toast.style.cssText = "position:fixed;bottom:20px;left:50%;transform:translateX(-50%);background:#e74c3c;color:white;padding:10px 24px;border-radius:8px;font-size:1rem;z-index:9999;box-shadow:0 2px 8px rgba(0,0,0,0.3);";
        document.body.appendChild(toast);
    }
    toast.textContent = msg;
    toast.style.display = "block";
    clearTimeout(toast._timeout);
    toast._timeout = setTimeout(() => { toast.style.display = "none"; }, 3000);
}


function showBestPlayers() {

}

document.addEventListener("DOMContentLoaded", async () => {
    initBoard();
    await startSignalR();
    updateStatus();
});