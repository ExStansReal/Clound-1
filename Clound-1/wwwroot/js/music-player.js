// music-player.js

const musicTracks = {
    home: '/music/main.mp3',
    cloud: '/music/cloud.mp3',
    rain: '/music/rain.mp3',
    snow: '/music/snow.mp3',
    sun: '/music/sun.mp3'
};

let fadeInterval = null;
let audioElement = null;
let volumeSlider = null; // ссылка на ползунок

// Сохранённая громкость (по умолчанию 0.7)
let savedVolume = localStorage.getItem('musicVolume');
if (savedVolume === null) savedVolume = 0.7;
else savedVolume = parseFloat(savedVolume);

// Обновление положения слайдера
function updateVolumeSlider(value) {
    if (volumeSlider) {
        volumeSlider.value = value;
    }
}

// Инициализация слайдера
function initVolumeSlider() {
    volumeSlider = document.getElementById('volumeSlider');
    if (!volumeSlider) return;

    // Устанавливаем начальное значение из сохранённого
    volumeSlider.value = savedVolume;

    // Обработчик изменения слайдера
    volumeSlider.addEventListener('input', function (e) {
        const val = parseFloat(e.target.value);
        if (audioElement) {
            audioElement.volume = val;
        }
        // Сохраняем в localStorage
        localStorage.setItem('musicVolume', val);
        // Если идёт fade, прерываем его
        if (fadeInterval) {
            clearInterval(fadeInterval);
            fadeInterval = null;
        }
    });
}

// Создание нового аудиоэлемента (чистый старт)
function createAudioElement(src) {
    const audio = new Audio();
    audio.src = src;
    audio.loop = true;
    audio.volume = 0; // начинаем с нуля
    audio.preload = 'auto';
    return audio;
}

// Плавное изменение громкости
function fadeVolume(targetVolume, duration, callback) {
    if (!audioElement) return;

    if (fadeInterval) clearInterval(fadeInterval);

    const steps = 20;
    const stepTime = duration / steps;
    const startVolume = audioElement.volume;
    const volumeStep = (targetVolume - startVolume) / steps;
    let currentStep = 0;

    fadeInterval = setInterval(() => {
        currentStep++;
        const newVol = startVolume + (volumeStep * currentStep);
        audioElement.volume = newVol;
        updateVolumeSlider(newVol); // обновляем слайдер

        if (currentStep >= steps) {
            clearInterval(fadeInterval);
            fadeInterval = null;
            audioElement.volume = targetVolume;
            updateVolumeSlider(targetVolume);
            if (callback) callback();
        }
    }, stepTime);
}

// Смена трека с плавным переходом
function changeTrack(newSrc) {
    // Если нет audioElement или первый запуск
    if (!audioElement) {
        audioElement = createAudioElement(newSrc);
        document.body.appendChild(audioElement);

        setTimeout(() => {
            const playPromise = audioElement.play();
            if (playPromise !== undefined) {
                playPromise
                    .then(() => {
                        // Стартуем с громкостью из savedVolume, но плавно
                        audioElement.volume = 0;
                        fadeVolume(savedVolume, 1000);
                    })
                    .catch(error => {
                        console.log('Автовоспроизведение заблокировано.');
                        showMusicStartButton(newSrc);
                    });
            }
        }, 100);
        return;
    }

    if (audioElement.src.includes(newSrc)) return;

    fadeVolume(0, 1000, () => {
        const oldAudio = audioElement;
        audioElement = createAudioElement(newSrc);
        document.body.appendChild(audioElement);

        setTimeout(() => {
            oldAudio.pause();
            oldAudio.remove();
        }, 100);

        setTimeout(() => {
            audioElement.play()
                .then(() => {
                    audioElement.volume = 0;
                    fadeVolume(savedVolume, 1000);
                })
                .catch(err => console.error('Не удалось запустить новый трек:', err));
        }, 150);
    });
}

// Определение текущей страницы
function getCurrentPage() {
    const identifier = document.getElementById('page-identifier');
    if (identifier) return identifier.dataset.page;
    return window.location.pathname.toLowerCase().includes('cloud') ? 'cloud' : 'home';
}

// Кнопка для ручного запуска
function showMusicStartButton(trackSrc) {
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
        if (!audioElement) {
            audioElement = createAudioElement(trackSrc);
            document.body.appendChild(audioElement);
        }
        audioElement.volume = 0;
        audioElement.play()
            .then(() => {
                fadeVolume(savedVolume, 1000);
                btn.remove();
            })
            .catch(err => console.error('Ошибка:', err));
    };

    document.body.appendChild(btn);
}

// Обработка переходов по ссылкам
function setupLinkHandler() {
    let isNavigating = false;

    document.addEventListener('click', function (e) {
        const link = e.target.closest('a');
        if (!link || isNavigating) return;

        const href = link.getAttribute('href');
        if (href && !href.startsWith('http') && !href.startsWith('#') && !href.startsWith('javascript')) {
            e.preventDefault();
            isNavigating = true;

            fadeVolume(0, 500, () => {
                window.location.href = href;
            });
        }
    });
}

// Функция для будущей погоды
function setMusicByWeather(weatherCondition) {
    const trackKey = musicTracks[weatherCondition] ? weatherCondition : 'home';
    changeTrack(musicTracks[trackKey]);
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function () {
    initVolumeSlider();             // инициализируем слайдер
    const page = getCurrentPage();
    changeTrack(musicTracks[page]);
    setupLinkHandler();
});