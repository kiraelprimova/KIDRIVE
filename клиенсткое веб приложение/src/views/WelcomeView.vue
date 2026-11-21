<template>
  <div class="landing">
    <header class="header">
      <a class="logo" href="#top" aria-label="KIDRIVE — на главную" @click="menuOpen = false">KI<span>DRIVE</span><i></i></a>
      <nav class="desktop-nav" aria-label="Основная навигация">
        <a href="#cars">Автомобили</a><a href="#how">Как это работает</a><a href="#about">О проекте</a>
      </nav>
      <div class="header-actions"><RouterLink to="/login" class="login-link">Войти</RouterLink><RouterLink to="/register" class="header-cta">Регистрация</RouterLink></div>
      <button class="menu-toggle" type="button" :aria-expanded="menuOpen" aria-controls="mobile-menu" :aria-label="menuOpen ? 'Закрыть меню' : 'Открыть меню'" @click="menuOpen = !menuOpen"><span></span><span></span></button>
    </header>
    <nav v-if="menuOpen" id="mobile-menu" class="mobile-menu" aria-label="Мобильная навигация">
      <a href="#cars" @click="menuOpen = false">Автомобили</a><a href="#how" @click="menuOpen = false">Как это работает</a><a href="#about" @click="menuOpen = false">О проекте</a><RouterLink to="/login" @click="menuOpen = false">Войти</RouterLink><RouterLink to="/register" @click="menuOpen = false">Регистрация</RouterLink>
    </nav>

    <main id="top">
      <section class="hero" aria-labelledby="hero-title">
        <div class="hero-shade"></div>
        <div class="hero-inner">
          <p class="overline hero-overline">KIDRIVE / АВТОМОБИЛИ И ТЕСТ-ДРАЙВЫ</p>
          <h1 id="hero-title">ВАША ДОРОГА<br>НАЧИНАЕТСЯ ЗДЕСЬ</h1>
          <p class="hero-description">Выбирайте автомобиль, знакомьтесь с деталями и записывайтесь на тест-драйв онлайн.</p>
          <div class="hero-actions"><a class="button button-light" href="#cars">Выбрать автомобиль</a><RouterLink class="button button-outline" to="/register">Записаться на тест-драйв</RouterLink></div>
        </div>
        <div class="hero-bottom"><span>ОТ ВЫБОРА К ВПЕЧАТЛЕНИЯМ</span><a href="#cars">ЛИСТАЙТЕ ВНИЗ</a></div>
      </section>

      <section id="cars" class="section models">
        <div class="section-heading"><div><p class="overline">ВЫБИРАЙТЕ СВОЁ</p><h2>АВТОМОБИЛИ<br>ДЛЯ ВАШИХ МАРШРУТОВ</h2></div><p>Знакомьтесь с автомобилями и находите тот, который подходит именно вам.</p></div>
        <div class="car-grid">
          <article v-for="car in cars" :key="car.name" class="car-card">
            <div class="car-image"><img :src="car.image" :alt="car.name"></div>
            <div class="car-copy"><p class="car-type">{{ car.type }}</p><h3>{{ car.name }}</h3><RouterLink to="/app/catalog" class="text-link">Смотреть в каталоге</RouterLink></div>
          </article>
        </div>
        <div class="models-footer"><p>Больше автомобилей и подробная информация — в каталоге KIDRIVE.</p><RouterLink class="button button-dark" to="/app/catalog">Перейти в каталог</RouterLink></div>
      </section>

      <section id="how" class="section how">
        <div class="section-heading"><div><p class="overline">ПРОСТО И УДОБНО</p><h2>ОТ ВЫБОРА<br>ДО ТЕСТ-ДРАЙВА</h2></div><p>Всё необходимое для знакомства с автомобилем — в вашем личном кабинете.</p></div>
        <div class="steps"><article><span>01</span><h3>Выберите автомобиль</h3><p>Посмотрите каталог, изучите характеристики и сохраните понравившиеся модели.</p></article><article><span>02</span><h3>Выберите время</h3><p>Оформите запись на тест-драйв на подходящую дату и время.</p></article><article><span>03</span><h3>Следите за записью</h3><p>Проверяйте статус бронирования и управляйте им в личном кабинете.</p></article></div>
      </section>

      <section id="about" class="about"><div class="about-photo" role="img" aria-label="Автомобиль на дороге"></div><div class="about-copy"><p class="overline">О ПРОЕКТЕ KIDRIVE</p><h2>ДОРОГА К ВАШЕМУ<br>АВТОМОБИЛЮ</h2><p>KIDRIVE помогает выбрать автомобиль и записаться на тест-драйв без лишних сложностей. Изучайте модели, сохраняйте избранное и управляйте записями в одном месте.</p><RouterLink class="button button-dark" to="/register">Начать знакомство</RouterLink></div></section>
    </main>
    <footer class="footer"><a class="logo footer-logo" href="#top">KI<span>DRIVE</span><i></i></a><p>Автомобили и тест-драйвы</p><div><a href="#cars">Автомобили</a><a href="#how">Как это работает</a><RouterLink to="/login">Войти</RouterLink></div><small>© KIDRIVE</small></footer>
  </div>
</template>

<script setup>
import { ref } from 'vue';
const menuOpen = ref(false);
const cars = [
  { name: 'AUDI RS7', type: 'СПОРТИВНЫЙ ЛИФТБЕК', image: '/Resources/catalog/car-08.jpg' },
  { name: 'BMW M5', type: 'СПОРТИВНЫЙ СЕДАН', image: '/Resources/catalog/car-05.jpg' },
  { name: 'AUDI Q3', type: 'КРОССОВЕР', image: '/Resources/catalog/car-07.jpg' },
];
</script>

<style scoped>
.landing { background:#fff; color:#171b24; font-family:'Segoe UI',Arial,sans-serif; }
.header { height:78px; display:flex; align-items:center; gap:70px; padding:0 5%; background:#fff; position:relative; z-index:5; }
.logo { color:#101d39; font-size:25px; font-weight:800; letter-spacing:.12em; text-decoration:none; white-space:nowrap; }
.logo span { color:#244b91; }.logo i { display:inline-block; width:6px; height:6px; margin:0 0 3px 3px; background:#3468b9; }
.desktop-nav,.header-actions { display:flex; align-items:center; gap:34px; }.desktop-nav a,.header-actions a { font-size:14px; font-weight:600; text-decoration:none; color:#232b39; }.desktop-nav a:hover,.login-link:hover { color:#315faa; }
.header-actions { margin-left:auto; gap:25px; }.header-actions .header-cta { background:#1e3a70; color:#fff; padding:15px 22px; }.header-cta:hover { background:#315a9c; }
.menu-toggle,.mobile-menu { display:none; }
.hero { min-height:calc(100svh - 78px); position:relative; color:white; background:#24374d url('/Resources/road-hero.jpg') center 60% / cover no-repeat; display:flex; flex-direction:column; justify-content:center; }
.hero-shade { position:absolute; inset:0; background:linear-gradient(90deg,rgba(8,19,34,.79) 0%,rgba(9,22,38,.54) 42%,rgba(7,17,29,.12) 100%),linear-gradient(0deg,rgba(7,17,29,.45),transparent 30%); }
.hero-inner { position:relative; width:100%; padding:40px 8%; }.overline { font-size:12px; font-weight:700; letter-spacing:.14em; color:#365c95; }.hero-overline { color:#d8e4f2; margin-bottom:25px; }
h1,h2,h3,p { margin:0; } h1 { font-size:clamp(40px,5.2vw,76px); font-weight:700; line-height:1.08; letter-spacing:-.025em; } .hero-description { margin-top:23px; max-width:470px; font-size:18px; line-height:1.6; color:#edf1f5; }
.hero-actions { display:flex; flex-wrap:wrap; gap:12px; margin-top:34px; }.button { min-height:54px; display:inline-flex; align-items:center; justify-content:center; padding:0 25px; font-size:14px; font-weight:650; text-decoration:none; transition:background-color .2s,color .2s,border-color .2s; }.button-light { color:#152033; background:#fff; border:1px solid #fff; }.button-light:hover { background:#e6ebf1; }.button-outline { color:white; border:1px solid rgba(255,255,255,.8); }.button-outline:hover { background:#fff; color:#152033; }.button-dark { background:#1e3a70; color:#fff; border:1px solid #1e3a70; }.button-dark:hover { background:#315a9c; border-color:#315a9c; }
.hero-bottom { position:absolute; bottom:30px; left:5%; right:5%; display:flex; justify-content:space-between; align-items:center; font-size:11px; font-weight:700; letter-spacing:.12em; color:#e5edf5; }.hero-bottom a { color:inherit; text-decoration:none; }.hero-bottom a span { font-size:20px; margin-left:10px; }
.section { padding:110px 7%; scroll-margin-top:20px; }.section-heading { display:flex; align-items:end; justify-content:space-between; gap:50px; margin-bottom:47px; }.section-heading h2,.about h2 { margin-top:18px; font-size:clamp(30px,3.3vw,48px); line-height:1.15; letter-spacing:-.025em; font-weight:700; }.section-heading > p { max-width:345px; color:#687384; font-size:16px; line-height:1.6; }
.car-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:20px; }.car-card { background:#f3f5f7; overflow:hidden; }.car-image { aspect-ratio:16 / 9; overflow:hidden; }.car-image img { width:100%; height:100%; object-fit:contain; transition:transform .35s ease; }.car-card:hover img { transform:scale(1.025); }.car-copy { padding:26px 28px 30px; }.car-type { font-size:11px; letter-spacing:.13em; color:#697689; font-weight:700; }.car-copy h3 { margin:10px 0 25px; font-size:25px; letter-spacing:.015em; }.text-link { font-size:14px; font-weight:650; color:#243c63; text-decoration:none; }.text-link:hover { color:#4275be; }
.models-footer { display:flex; justify-content:space-between; align-items:center; gap:25px; margin-top:36px; }.models-footer p { color:#697689; font-size:15px; line-height:1.5; }
.how { background:#f1f3f6; }.steps { display:grid; grid-template-columns:repeat(3,1fr); gap:36px; }.steps article { border-top:1px solid #b6bfca; padding-top:24px; }.steps span { font-size:13px; color:#4771a8; font-weight:700; }.steps h3 { font-size:20px; margin:36px 0 12px; }.steps p { color:#657183; font-size:16px; line-height:1.6; max-width:340px; }
.about { display:grid; grid-template-columns:1fr 1fr; min-height:480px; scroll-margin-top:20px; }.about-photo { min-height:420px; background:url('/Resources/road-hero.jpg') center 67% / cover; }.about-copy { padding:80px 11%; display:flex; flex-direction:column; align-items:flex-start; justify-content:center; }.about-copy > p:not(.overline) { max-width:470px; margin:25px 0 30px; color:#657183; font-size:16px; line-height:1.7; }
.footer { background:#101a2a; color:#c6d0df; padding:55px 7% 34px; display:grid; grid-template-columns:1fr 1fr; gap:20px; }.footer-logo { color:white; }.footer-logo span { color:#93b2e1; }.footer p { grid-column:1; font-size:14px; }.footer div { grid-column:2; grid-row:1 / 3; display:flex; justify-content:flex-end; flex-wrap:wrap; gap:28px; }.footer div a { color:#e2e9f2; text-decoration:none; font-size:14px; }.footer small { grid-column:1 / -1; border-top:1px solid #344052; padding-top:24px; margin-top:20px; font-size:12px; }
a:focus-visible,button:focus-visible { outline:3px solid #77aaff; outline-offset:4px; }
@media(max-width:900px) { .header { gap:25px; }.desktop-nav { gap:18px; }.header-actions { gap:14px; }.header-actions .header-cta { padding:13px; }.section { padding:80px 5%; }.car-copy { padding:22px; }.about-copy { padding:60px 8%; } }
@media(max-width:700px) { .header { height:68px; padding:0 20px; }.desktop-nav,.header-actions { display:none; }.menu-toggle { margin-left:auto; display:flex; width:42px; height:42px; background:none; flex-direction:column; justify-content:center; gap:7px; padding:8px; }.menu-toggle span { display:block; width:25px; height:2px; background:#14213c; }.mobile-menu { position:absolute; z-index:6; left:0; right:0; display:flex; flex-direction:column; background:white; padding:10px 20px 22px; box-shadow:0 10px 20px #0c1b2d20; }.mobile-menu a { padding:13px 0; color:#18253a; font-size:16px; text-decoration:none; border-bottom:1px solid #edf0f4; }.hero { min-height:calc(100svh - 68px); background-position:55% 63%; }.hero-shade { background:linear-gradient(90deg,rgba(8,19,34,.8),rgba(8,19,34,.28)),linear-gradient(0deg,rgba(8,19,34,.45),transparent 50%); }.hero-inner { padding:35px 22px 90px; }.hero-overline { line-height:1.5; } h1 { font-size:clamp(35px,8vw,52px); }.hero-description { font-size:16px; }.hero-actions { flex-direction:column; align-items:stretch; max-width:330px; }.hero-bottom { left:22px; right:22px; }.hero-bottom > span { display:none; }.section { padding:72px 22px; }.section-heading { display:block; margin-bottom:30px; }.section-heading > p { margin-top:20px; }.car-grid { grid-template-columns:1fr; }.models-footer { align-items:flex-start; flex-direction:column; }.steps { grid-template-columns:1fr; gap:30px; }.steps h3 { margin-top:18px; }.about { grid-template-columns:1fr; }.about-photo { min-height:300px; }.about-copy { padding:65px 22px; }.footer { grid-template-columns:1fr; padding:45px 22px 30px; }.footer div { grid-column:1; grid-row:auto; justify-content:flex-start; gap:20px; }.footer small { grid-column:1; } }
@media(prefers-reduced-motion:reduce) { .car-image img,.button { transition:none; } }
</style>
