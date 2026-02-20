//let selectedCity = {
//    name: 'Москва',
//    temp: 0,
//    wind: 0,
//    desc: 'неизвестно'
//};

//// Выделение выбранного города
//document.querySelectorAll('.city-item').forEach(item => {
//    item.addEventListener('click', function () {
//        document.querySelectorAll('.city-item').forEach(el => el.classList.remove('selected'));
//        this.classList.add('selected');

//        selectedCity = {
//            name: this.dataset.city,
//            temp: this.dataset.temp,
//            wind: this.dataset.wind,
//            desc: this.dataset.desc
//        };

//        // Обновляем ссылку на облака
//        const cloudLink = document.getElementById('goToCloud');
//        cloudLink.href = `/Cloud?city=${encodeURIComponent(selectedCity.name)}&weather=${encodeURIComponent(selectedCity.desc)}`;
//    });
//});

//// Выбрать первый город по умолчанию
//document.addEventListener('DOMContentLoaded', function () {
//    const firstCity = document.querySelector('.city-item');
//    if (firstCity) {
//        firstCity.click();
//    }
//});

//// Обновление погоды для всех городов
//document.getElementById('refreshWeather')?.addEventListener('click', async function () {
//    try {
//        const response = await fetch('@Url.Action("GetAllWeatherJson", "Home")');
//        const data = await response.json();


       
//        if (data.error) {
//            console.error('Ошибка от сервера:', data.error);
//            return;
//        }
//        // если data - массив, то обновляем список
//        if (!Array.isArray(data)) {
//            console.error('Неожиданный формат данных:', data);
//            return;
//        }

//        // Обновляем содержимое списка
//        const cityList = document.querySelector('.city-list');
//        cityList.innerHTML = '';

//        data.forEach(cityWeather => {
//            const div = document.createElement('div');
//            div.className = 'city-item p-2 mb-2 rounded';
//            div.dataset.city = cityWeather.city;
//            div.dataset.temp = cityWeather.temperature;
//            div.dataset.wind = cityWeather.windSpeed;
//            div.dataset.desc = cityWeather.description;
//            div.style.cssText = 'cursor: pointer; background-color: #f8f9fa; border: 1px solid #dee2e6;';
//            div.innerHTML = `
//                    <div><strong>${cityWeather.city}</strong></div>
//                    <div>🌡 ${cityWeather.temperature}°C | 💨 ${cityWeather.windSpeed} км/ч</div>
//                    <div>${cityWeather.description}</div>
//                `;
//            div.addEventListener('click', function () {
//                document.querySelectorAll('.city-item').forEach(el => el.classList.remove('selected'));
//                this.classList.add('selected');
//                selectedCity = {
//                    name: this.dataset.city,
//                    temp: this.dataset.temp,
//                    wind: this.dataset.wind,
//                    desc: this.dataset.desc
//                };
//                document.getElementById('goToCloud').href = `/Cloud?city=${encodeURIComponent(selectedCity.name)}&weather=${encodeURIComponent(selectedCity.desc)}`;
//            });
//            cityList.appendChild(div);
//        });

//        // Выбрать первый город после обновления
//        const first = document.querySelector('.city-item');
//        if (first) first.click();
//    } catch (error) {
//        console.error('Ошибка обновления погоды:', error);
//    }
//});