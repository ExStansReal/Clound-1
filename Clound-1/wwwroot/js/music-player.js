// music-player.js

// Настройки музыки для разных страниц
const musicTracks = {
    home: '/music/home-theme.mp3',
    cloud: '/music/cloud-theme.mp3'
};

let fadeInterval = null;

// Функция плавного изменения громкости (fade)
function fadeVolume(targetVolume, duration, callback) {
    const audio = document.getElementById('bg-music');
    if (!audio) return;

    if (fadeInterval) clearInterval(fadeInterval);

    const steps = 20;
    const stepTime = duration / steps;
    const startVolume = audio.volume;
    const volumeStep = (targetVolume - startVolume) / steps;
    let currentStep = 0;

    fadeInterval = setInterval(() => {
        currentStep++;
        audio.volume = startVolume + (volumeStep * currentStep);

        if (currentStep >= steps) {
            clearInterval(fadeInterval);
            fadeInterval = null;
            audio.volume = targetVolume;
            if (callback) callback();
        }
    }, stepTime);
}

// Функция смены трека с плавным переходом
function changeTrack(newSrc) {
    const audio = document.getElementById('bg-music');
    if (!audio) return;

    // Если это первый запуск (audio ещё не инициализирован)
    if (!audio.src || audio.src === '') {
        audio.src = newSrc;
        audio.volume = 0.7;

        // Пытаемся воспроизвести
        const playPromise = audio.play();
        if (playPromise !== undefined) {
            playPromise.catch(error => {
                console.log('Автовоспроизведение заблокировано. Показываем кнопку.');
                showMusicStartButton();
            });
        }
        return;
    }

    // Если трек уже играет, делаем fade out, меняем трек, затем fade in
    if (audio.src.includes(newSrc)) return;

    fadeVolume(0, 1000, () => {
        audio.src = newSrc;
        audio.play();
        fadeVolume(0.7, 1000);
    });
}

// Определяем текущую страницу по data-атрибуту
function getCurrentPage() {
    const identifier = document.getElementById('page-identifier');
    if (identifier) {
        return identifier.dataset.page;
    }
    // Fallback на определение по URL
    const path = window.location.pathname.toLowerCase();
    return path.includes('cloud') ? 'cloud' : 'home';
}

// Кнопка для запуска музыки (если браузер заблокировал автовоспроизведение)
function showMusicStartButton() {
    // Проверяем, не была ли кнопка уже добавлена
    if (document.getElementById('music-start-btn')) return;

    const btn = document.createElement('button');
    btn.id = 'music-start-btn';
    btn.innerHTML = '🎵 Включить музыку';
    btn.style.position = 'fixed';
    btn.style.bottom = '20px';
    btn.style.right = '20px';
    btn.style.zIndex = '1000';
    btn.style.padding = '10px 20px';
    btn.style.background = '#4CAF50';
    btn.style.color = 'white';
    btn.style.border = 'none';
    btn.style.borderRadius = '30px';
    btn.style.cursor = 'pointer';
    btn.style.boxShadow = '0 4px 15px rgba(0,0,0,0.2)';

    btn.onclick = function () {
        const audio = document.getElementById('bg-music');
        audio.play();
        audio.volume = 0.7;
        btn.remove();
    };

    document.body.appendChild(btn);
}

// Обработка переходов по ссылкам
function setupLinkHandler() {
    document.addEventListener('click', function (e) {
        const link = e.target.closest('a');
        if (!link) return;

        const href = link.getAttribute('href');
        if (href && !href.startsWith('http') && !href.startsWith('#') && !href.startsWith('javascript')) {
            e.preventDefault();

            // Плавно убираем звук
            fadeVolume(0, 500, () => {
                window.location.href = href;
            });
        }
    });
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function () {
    const page = getCurrentPage();
    changeTrack(musicTracks[page]);
    setupLinkHandler();
});