// Chain Reaction Audio & Haptic Feedback Engine
// Uses Web Audio API for zero-latency procedural synthesis + confetti for victory celebrations

class AudioManager {
    constructor() {
        this.ctx = null;
        this.isMuted = false;
        this.explosionChain = 0;
        this.chainTimeout = null;
    }

    init() {
        if (!this.ctx) {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (AudioCtx) {
                this.ctx = new AudioCtx();
            }
        }
        if (this.ctx && this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    // Play pleasant pop when placing an orb
    playPop(pitchMultiplier = 1.0) {
        if (this.isMuted) return;
        this.init();
        if (!this.ctx) return;

        try {
            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            const freq = 420 * pitchMultiplier;
            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, now);
            osc.frequency.exponentialRampToValueAtTime(freq * 1.6, now + 0.08);

            gain.gain.setValueAtTime(0.22, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.09);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now);
            osc.stop(now + 0.1);
        } catch (e) {
            console.warn('Audio error:', e);
        }
    }

    // Play explosion sound with pitch scaling based on chain reaction count
    playExplosion(chainLevel = 1) {
        if (this.isMuted) return;
        this.init();
        if (!this.ctx) return;

        try {
            const now = this.ctx.currentTime;
            
            // 1. Bass punch
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            
            // Base frequency rises slightly with chain level for dopamine rush
            const baseFreq = Math.min(280, 110 + (chainLevel * 14));
            osc.type = 'triangle';
            osc.frequency.setValueAtTime(baseFreq, now);
            osc.frequency.exponentialRampToValueAtTime(30, now + 0.16);

            gain.gain.setValueAtTime(0.35, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now);
            osc.stop(now + 0.2);

            // 2. High crunch / pop for crispness
            const popOsc = this.ctx.createOscillator();
            const popGain = this.ctx.createGain();
            popOsc.type = 'sine';
            const popFreq = Math.min(1200, 450 + (chainLevel * 45));
            popOsc.frequency.setValueAtTime(popFreq, now);
            popOsc.frequency.exponentialRampToValueAtTime(80, now + 0.08);

            popGain.gain.setValueAtTime(0.25, now);
            popGain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);

            popOsc.connect(popGain);
            popGain.connect(this.ctx.destination);

            popOsc.start(now);
            popOsc.stop(now + 0.09);
        } catch (e) {
            console.warn('Audio explosion error:', e);
        }
    }

    // Dramatic descending chime when a player is eliminated
    playEliminate() {
        if (this.isMuted) return;
        this.init();
        if (!this.ctx) return;

        try {
            const notes = [440, 392, 349, 293];
            notes.forEach((freq, idx) => {
                const now = this.ctx.currentTime + (idx * 0.1);
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();
                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(freq, now);
                gain.gain.setValueAtTime(0.18, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);
                osc.connect(gain);
                gain.connect(this.ctx.destination);
                osc.start(now);
                osc.stop(now + 0.2);
            });
        } catch (e) {}
    }

    // Victory fanfare on game over
    playVictory() {
        if (this.isMuted) return;
        this.init();
        if (!this.ctx) return;

        try {
            const melody = [
                { f: 523.25, d: 0.12 }, // C5
                { f: 659.25, d: 0.12 }, // E5
                { f: 783.99, d: 0.12 }, // G5
                { f: 1046.50, d: 0.35 } // C6
            ];
            let offset = 0;
            melody.forEach((note) => {
                const now = this.ctx.currentTime + offset;
                const osc = this.ctx.createOscillator();
                const gain = this.ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(note.f, now);
                gain.gain.setValueAtTime(0.28, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + note.d);
                osc.connect(gain);
                gain.connect(this.ctx.destination);
                osc.start(now);
                osc.stop(now + note.d + 0.05);
                offset += note.d + 0.03;
            });
        } catch (e) {}
    }
}

const audioMgr = new AudioManager();

// Global blazor feedback functions
window.blazorFunctions = {
    BhukampLao: function (shouldVibrate, shouldPlay, chainLevel = 1) {
        if (shouldVibrate && window.navigator && window.navigator.vibrate) {
            try {
                window.navigator.vibrate(Math.min(45, 18 + chainLevel * 3));
            } catch (e) {}
        }
        if (shouldPlay) {
            audioMgr.playExplosion(chainLevel);
        }
    },
    PlayPop: function (pitch = 1.0) {
        audioMgr.playPop(pitch);
    },
    PlayExplosion: function (chainLevel = 1) {
        audioMgr.playExplosion(chainLevel);
    },
    PlayEliminate: function () {
        audioMgr.playEliminate();
    },
    PlayVictory: function () {
        audioMgr.playVictory();
        window.blazorFunctions.LaunchConfetti();
    },
    SetMute: function (muted) {
        audioMgr.isMuted = muted;
    },
    // Confetti burst for victory celebrations
    LaunchConfetti: function () {
        const duration = 3000;
        const animationEnd = Date.now() + duration;
        const canvas = document.createElement('canvas');
        canvas.id = 'victory-confetti-canvas';
        canvas.style.position = 'fixed';
        canvas.style.inset = '0';
        canvas.style.width = '100vw';
        canvas.style.height = '100vh';
        canvas.style.pointerEvents = 'none';
        canvas.style.zIndex = '99999';
        document.body.appendChild(canvas);

        const ctx = canvas.getContext('2d');
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        const colors = ['#f59e0b', '#ef4444', '#10b981', '#3b82f6', '#8b5cf6', '#ec4899', '#fbbf24'];
        const particles = [];
        for (let i = 0; i < 90; i++) {
            particles.push({
                x: window.innerWidth * 0.5 + (Math.random() - 0.5) * 200,
                y: window.innerHeight * 0.4 + (Math.random() - 0.5) * 100,
                vx: (Math.random() - 0.5) * 12,
                vy: (Math.random() - 0.9) * 14 - 3,
                size: Math.random() * 8 + 6,
                color: colors[Math.floor(Math.random() * colors.length)],
                rotation: Math.random() * 360,
                rotSpeed: (Math.random() - 0.5) * 10,
                opacity: 1
            });
        }

        function frame() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            const remaining = (animationEnd - Date.now()) / duration;
            if (remaining <= 0) {
                canvas.remove();
                return;
            }

            particles.forEach(p => {
                p.x += p.vx;
                p.y += p.vy;
                p.vy += 0.35; // gravity
                p.rotation += p.rotSpeed;
                p.opacity = Math.max(0, remaining);

                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate((p.rotation * Math.PI) / 180);
                ctx.fillStyle = p.color;
                ctx.globalAlpha = p.opacity;
                ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size * 0.6);
                ctx.restore();
            });

            requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);
    },
    // LocalStorage helper for announcement popup
    HasSeenReleasePopup: function () {
        try {
            return localStorage.getItem('cr_release_popup_dismissed') === 'true';
        } catch (e) {
            return false;
        }
    },
    SetSeenReleasePopup: function () {
        try {
            localStorage.setItem('cr_release_popup_dismissed', 'true');
        } catch (e) {}
    }
};