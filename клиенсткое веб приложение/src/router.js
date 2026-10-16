import { createRouter, createWebHistory } from 'vue-router';
import WelcomeView from './views/WelcomeView.vue';
import LoginView from './views/LoginView.vue';
import RegisterView from './views/RegisterView.vue';
import ClientLayout from './views/ClientLayout.vue';
import CatalogView from './views/CatalogView.vue';
import MyBookingsView from './views/MyBookingsView.vue';
import ProfileView from './views/ProfileView.vue';
import { authState } from './auth';

const routes = [
  {
    path: '/',
    name: 'welcome',
    component: WelcomeView,
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView,
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView,
  },
  {
    path: '/app',
    component: ClientLayout,
    children: [
      {
        path: '',
        redirect: '/app/catalog',
      },
      {
        path: 'catalog',
        name: 'catalog',
        component: CatalogView,
      },
      {
        path: 'bookings',
        name: 'bookings',
        component: MyBookingsView,
      },
      {
        path: 'profile',
        name: 'profile',
        component: ProfileView,
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
