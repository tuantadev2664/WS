// Mini Battleship Game JavaScript
document.addEventListener('DOMContentLoaded', function() {
    let gameState = null;
    let isProcessing = false;

    // Initialize game state
    initializeGame();

    // Sound effects
    const playSound = (soundType) => {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        
        switch(soundType) {
            case 'hit':
                // Explosion sound
                const hitOscillator = audioContext.createOscillator();
                const hitGain = audioContext.createGain();
                hitOscillator.connect(hitGain);
                hitGain.connect(audioContext.destination);
                hitOscillator.frequency.setValueAtTime(200, audioContext.currentTime);
                hitOscillator.frequency.exponentialRampToValueAtTime(50, audioContext.currentTime + 0.3);
                hitGain.gain.setValueAtTime(0.3, audioContext.currentTime);
                hitGain.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);
                hitOscillator.start(audioContext.currentTime);
                hitOscillator.stop(audioContext.currentTime + 0.3);
                break;
                
            case 'miss':
                // Water splash sound
                const missOscillator = audioContext.createOscillator();
                const missGain = audioContext.createGain();
                missOscillator.connect(missGain);
                missGain.connect(audioContext.destination);
                missOscillator.frequency.setValueAtTime(800, audioContext.currentTime);
                missOscillator.frequency.exponentialRampToValueAtTime(200, audioContext.currentTime + 0.2);
                missGain.gain.setValueAtTime(0.2, audioContext.currentTime);
                missGain.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);
                missOscillator.start(audioContext.currentTime);
                missOscillator.stop(audioContext.currentTime + 0.2);
                break;
                
            case 'win':
                // Victory fanfare
                const winOscillator1 = audioContext.createOscillator();
                const winOscillator2 = audioContext.createOscillator();
                const winGain = audioContext.createGain();
                winOscillator1.connect(winGain);
                winOscillator2.connect(winGain);
                winGain.connect(audioContext.destination);
                winOscillator1.frequency.setValueAtTime(523, audioContext.currentTime); // C5
                winOscillator2.frequency.setValueAtTime(659, audioContext.currentTime); // E5
                winOscillator1.frequency.setValueAtTime(659, audioContext.currentTime + 0.2); // E5
                winOscillator2.frequency.setValueAtTime(784, audioContext.currentTime + 0.2); // G5
                winOscillator1.frequency.setValueAtTime(784, audioContext.currentTime + 0.4); // G5
                winOscillator2.frequency.setValueAtTime(1047, audioContext.currentTime + 0.4); // C6
                winGain.gain.setValueAtTime(0.2, audioContext.currentTime);
                winGain.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.8);
                winOscillator1.start(audioContext.currentTime);
                winOscillator2.start(audioContext.currentTime);
                winOscillator1.stop(audioContext.currentTime + 0.8);
                winOscillator2.stop(audioContext.currentTime + 0.8);
                break;
        }
    };

    // Initialize game state
    async function initializeGame() {
        try {
            console.log('Initializing game...');
            const response = await fetch('/Home/GetGameState');
            const data = await response.json();
            console.log('Game state response:', data);
            
            if (data.success) {
                gameState = data;
                updateGameUI();
            } else {
                console.log('No game found, checking debug...');
                const debugResponse = await fetch('/Home/DebugGame');
                const debugData = await debugResponse.json();
                console.log('Debug response:', debugData);
            }
        } catch (error) {
            console.error('Error initializing game:', error);
        }
    }

    // Update game UI based on current state
    function updateGameUI() {
        if (!gameState) return;

        // Update game info
        updateGameInfo();
        
        // Update boards
        updateBoards();
        
        // Show/hide controls
        updateControls();
    }

    // Update game information display
    function updateGameInfo() {
        const gameMode = document.querySelector('.game-mode');
        const gameTurn = document.querySelector('.game-turn');
        const gameStats = document.querySelector('.game-stats');
        
        if (gameMode) gameMode.textContent = `🎮 Chế độ: ${gameState.mode}`;
        if (gameTurn) gameTurn.textContent = `⚡ Lượt: ${gameState.active}`;
        if (gameStats) {
            gameStats.innerHTML = `
                <span>📊 Player A: ${gameState.shotsA} phát</span>
                <span>📊 Player B: ${gameState.shotsB} phát</span>
            `;
        }
    }

    // Update game boards
    function updateBoards() {
        // This would need to be implemented based on your board rendering
        // For now, we'll just reload the boards section
        loadBoards();
    }

    // Load boards via AJAX
    async function loadBoards() {
        try {
            const response = await fetch('/Home/GetBoards');
            if (response.ok) {
                const html = await response.text();
                const boardsContainer = document.querySelector('.boards-container');
                if (boardsContainer) {
                    boardsContainer.innerHTML = html;
                    attachBoardEvents();
                }
            }
        } catch (error) {
            console.error('Error loading boards:', error);
        }
    }

    // Attach events to board elements
    function attachBoardEvents() {
        const fireButtons = document.querySelectorAll('.fire-btn');
        fireButtons.forEach(button => {
            button.addEventListener('click', handleFire);
        });
    }

    // Handle fire action
    async function handleFire(e) {
        e.preventDefault();
        
        if (isProcessing) return;
        isProcessing = true;

        const button = e.target;
        const row = parseInt(button.getAttribute('data-row'));
        const col = parseInt(button.getAttribute('data-col'));

        console.log(`Firing at row=${row}, col=${col}`);

        // Add ripple effect
        addRippleEffect(button, e);

        try {
            const response = await fetch('/Home/Fire', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ r: row, c: col })
            });

            const result = await response.json();
            console.log('Fire result:', result);
            
            if (result.success) {
                // Update UI based on result
                updateCellState(button, result.hit);
                
                // Play sound
                playSound(result.hit ? 'hit' : 'miss');
                
                // Show flash message
                showFlashMessage(result.hit ? '🎯 Bắn trúng!' : '🌊 Bắn trượt!', result.hit ? 'success' : 'error');
                
                // Update game state
                gameState = {
                    ...gameState,
                    shotsA: result.shotsA,
                    shotsB: result.shotsB,
                    active: result.active,
                    gameOver: result.gameOver,
                    winner: result.winner
                };
                
                updateGameUI();
                
                // Handle bot shot if it happened
                if (result.botShot) {
                    // Add delay only for bot thinking
                    setTimeout(() => {
                        handleBotShot(result.botRow, result.botCol, result.botHit);
                    }, 1000 + Math.random() * 2000); // 1-3 seconds delay for bot thinking
                }
                
                // Check for game over
                if (result.gameOver) {
                    setTimeout(() => showWinnerModal(result.winner), 1000);
                }
            } else {
                showFlashMessage(result.message, 'error');
            }
        } catch (error) {
            console.error('Error firing:', error);
            showFlashMessage('Lỗi kết nối!', 'error');
        } finally {
            isProcessing = false;
        }
    }

    // Update cell state after firing
    function updateCellState(cell, hit) {
        cell.disabled = true;
        cell.style.pointerEvents = 'none';
        
        if (hit) {
            cell.classList.add('hit');
            cell.innerHTML = '💥';
        } else {
            cell.classList.add('miss');
            cell.innerHTML = '🌊';
        }
    }

    // Add ripple effect to button
    function addRippleEffect(button, event) {
        const ripple = document.createElement('span');
        ripple.classList.add('ripple');
        button.appendChild(ripple);
        
        const rect = button.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = (event.clientX - rect.left - size / 2) + 'px';
        ripple.style.top = (event.clientY - rect.top - size / 2) + 'px';
        
        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    // Show flash message
    function showFlashMessage(message, type) {
        const existingFlash = document.querySelector('.flash');
        if (existingFlash) {
            existingFlash.remove();
        }

        const flash = document.createElement('div');
        flash.className = `flash ${type}`;
        flash.textContent = message;
        
        const gameContainer = document.querySelector('.battleship-game');
        if (gameContainer) {
            gameContainer.insertBefore(flash, gameContainer.firstChild);
            
            setTimeout(() => {
                flash.remove();
            }, 3000);
        }
    }

    // Handle bot shot animation
    function handleBotShot(row, col, hit) {
        console.log(`Bot shot at (${row}, ${col}), hit: ${hit}`);
        
        // Find the cell in your board (opponent's view)
        const yourBoard = document.querySelector('.board-section:last-child .game-grid');
        if (yourBoard) {
            const cell = yourBoard.querySelector(`tr:nth-child(${row + 2}) td:nth-child(${col + 2})`);
            if (cell) {
                // Show bot thinking message briefly
                showFlashMessage('🤖 Bot đang bắn...', 'info');
                
                // Add bot shot animation immediately
                cell.style.animation = 'botShotPulse 0.5s ease-in-out';
                
                // Update cell state
                if (hit) {
                    cell.classList.add('hit');
                    cell.innerHTML = '💥';
                    showFlashMessage('🤖 Bot bắn trúng!', 'error');
                    playSound('hit');
                } else {
                    cell.classList.add('miss');
                    cell.innerHTML = '🌊';
                    showFlashMessage('🤖 Bot bắn trượt!', 'info');
                    playSound('miss');
                }
                
                // Remove animation after it completes
                setTimeout(() => {
                    cell.style.animation = '';
                }, 500);
            }
        }
    }

    // Show winner modal
    function showWinnerModal(winner) {
        const modal = document.createElement('div');
        modal.className = 'winner-modal';
        modal.innerHTML = `
            <div class="winner-content">
                <h2>🎉 ${winner} Thắng! 🎉</h2>
                <p>📊 Thống kê: Player A: ${gameState.shotsA} phát | Player B: ${gameState.shotsB} phát</p>
                <div class="winner-actions">
                    <a class="btn btn-primary" href="/new?mode=bot">🤖 Chơi lại vs Bot</a>
                    <a class="btn btn-secondary" href="/new?mode=hotseat">👥 Chơi lại 2 người</a>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        playSound('win');
        createConfetti();
    }

    // Update controls visibility
    function updateControls() {
        const switchButton = document.querySelector('.switch-turn-btn');
        if (switchButton) {
            if (gameState.mode === 'HotSeat') {
                switchButton.style.display = 'block';
            } else {
                switchButton.style.display = 'none';
            }
        }
    }

    // Handle switch turn
    async function handleSwitchTurn() {
        if (isProcessing) return;
        isProcessing = true;

        try {
            const response = await fetch('/Home/SwitchTurn', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            const result = await response.json();
            
            if (result.success) {
                gameState.active = result.active;
                updateGameUI();
                showFlashMessage('🔄 Đã đổi lượt!', 'info');
            } else {
                showFlashMessage(result.message, 'error');
            }
        } catch (error) {
            console.error('Error switching turn:', error);
            showFlashMessage('Lỗi kết nối!', 'error');
        } finally {
            isProcessing = false;
        }
    }

    // Attach switch turn button event
    const switchTurnButton = document.querySelector('.switch-turn-btn');
    if (switchTurnButton) {
        switchTurnButton.addEventListener('click', handleSwitchTurn);
    }

    // Add click effects to fire buttons (legacy support)
    const fireButtons = document.querySelectorAll('.fire-btn');
    fireButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            // Add ripple effect
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            this.appendChild(ripple);
            
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
            ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Add CSS for ripple effect
    const style = document.createElement('style');
    style.textContent = `
        .fire-btn {
            position: relative;
            overflow: hidden;
        }
        
        .ripple {
            position: absolute;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            transform: scale(0);
            animation: ripple-animation 0.6s linear;
            pointer-events: none;
        }
        
        @keyframes ripple-animation {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
        
        .flash {
            animation: slideInBounce 0.8s ease-out;
        }
        
        @keyframes slideInBounce {
            0% {
                opacity: 0;
                transform: translateY(-30px) scale(0.8);
            }
            50% {
                transform: translateY(5px) scale(1.05);
            }
            100% {
                opacity: 1;
                transform: translateY(0) scale(1);
            }
        }
        
        .winner-modal {
            animation: fadeInScale 0.6s ease-out;
        }
        
        @keyframes fadeInScale {
            from {
                opacity: 0;
                transform: scale(0.7);
            }
            to {
                opacity: 1;
                transform: scale(1);
            }
        }
        
        .game-grid td {
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        .game-grid td:hover {
            transform: scale(1.1) rotate(2deg);
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
        }
        
        .ship {
            animation: shipFloat 3s ease-in-out infinite;
        }
        
        @keyframes shipFloat {
            0%, 100% { transform: translateY(0px); }
            50% { transform: translateY(-3px); }
        }
        
        .hit {
            animation: hitPulse 0.6s ease-out;
        }
        
        @keyframes hitPulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.2); }
            100% { transform: scale(1); }
        }
        
        .miss {
            animation: missSplash 0.4s ease-out;
        }
        
        @keyframes missSplash {
            0% { transform: scale(0.8); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
        
        @keyframes botShotPulse {
            0% { 
                transform: scale(1);
                box-shadow: 0 0 0 0 rgba(255, 0, 0, 0.7);
            }
            50% { 
                transform: scale(1.1);
                box-shadow: 0 0 0 10px rgba(255, 0, 0, 0);
            }
            100% { 
                transform: scale(1);
                box-shadow: 0 0 0 0 rgba(255, 0, 0, 0);
            }
        }
    `;
    document.head.appendChild(style);

    // Check for flash messages and play appropriate sounds
    const flashMessages = document.querySelectorAll('.flash');
    flashMessages.forEach(flash => {
        if (flash.classList.contains('success')) {
            playSound('hit');
        } else if (flash.classList.contains('error')) {
            playSound('miss');
        }
    });

    // Check for winner modal and play victory sound
    const winnerModal = document.querySelector('.winner-modal');
    if (winnerModal) {
        setTimeout(() => playSound('win'), 500);
    }

    // Add particle effects for hits
    const hitCells = document.querySelectorAll('.hit');
    hitCells.forEach(cell => {
        cell.addEventListener('animationend', function() {
            createParticles(this);
        });
    });

    function createParticles(element) {
        const rect = element.getBoundingClientRect();
        for (let i = 0; i < 8; i++) {
            const particle = document.createElement('div');
            particle.style.position = 'fixed';
            particle.style.left = rect.left + rect.width / 2 + 'px';
            particle.style.top = rect.top + rect.height / 2 + 'px';
            particle.style.width = '4px';
            particle.style.height = '4px';
            particle.style.background = '#ff6b6b';
            particle.style.borderRadius = '50%';
            particle.style.pointerEvents = 'none';
            particle.style.zIndex = '1000';
            
            const angle = (Math.PI * 2 * i) / 8;
            const velocity = 50 + Math.random() * 30;
            const vx = Math.cos(angle) * velocity;
            const vy = Math.sin(angle) * velocity;
            
            document.body.appendChild(particle);
            
            let x = 0, y = 0;
            const animate = () => {
                x += vx * 0.016;
                y += vy * 0.016;
                particle.style.transform = `translate(${x}px, ${y}px)`;
                particle.style.opacity = 1 - Math.sqrt(x*x + y*y) / 100;
                
                if (particle.style.opacity > 0) {
                    requestAnimationFrame(animate);
                } else {
                    particle.remove();
                }
            };
            animate();
        }
    }

    // Add keyboard navigation
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            const focusedButton = document.activeElement;
            if (focusedButton && focusedButton.classList.contains('fire-btn')) {
                focusedButton.click();
            }
        }
    });

    // Add loading animation for form submissions
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.style.opacity = '0.6';
                submitButton.style.pointerEvents = 'none';
                submitButton.innerHTML = '⏳ Đang xử lý...';
            }
        });
    });

    // Add confetti effect for winner
    if (winnerModal) {
        setTimeout(() => {
            createConfetti();
        }, 1000);
    }

    function createConfetti() {
        const colors = ['#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#feca57', '#ff9ff3'];
        const confettiCount = 100;
        
        for (let i = 0; i < confettiCount; i++) {
            const confetti = document.createElement('div');
            confetti.style.position = 'fixed';
            confetti.style.left = Math.random() * window.innerWidth + 'px';
            confetti.style.top = '-10px';
            confetti.style.width = Math.random() * 10 + 5 + 'px';
            confetti.style.height = confetti.style.width;
            confetti.style.background = colors[Math.floor(Math.random() * colors.length)];
            confetti.style.borderRadius = Math.random() > 0.5 ? '50%' : '0';
            confetti.style.pointerEvents = 'none';
            confetti.style.zIndex = '1001';
            confetti.style.transform = `rotate(${Math.random() * 360}deg)`;
            
            document.body.appendChild(confetti);
            
            const fallSpeed = Math.random() * 3 + 2;
            const rotationSpeed = Math.random() * 10 - 5;
            let y = 0;
            let rotation = 0;
            
            const animate = () => {
                y += fallSpeed;
                rotation += rotationSpeed;
                confetti.style.transform = `translateY(${y}px) rotate(${rotation}deg)`;
                confetti.style.opacity = 1 - (y / window.innerHeight);
                
                if (y < window.innerHeight + 50) {
                    requestAnimationFrame(animate);
                } else {
                    confetti.remove();
                }
            };
            animate();
        }
    }
});
