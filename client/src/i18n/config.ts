import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import en from './locales/en.json';
import az from './locales/az.json';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      az: { translation: az },
    },
    lng: 'en',
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

// Set default language to English
if (typeof window !== 'undefined') {
  if (!localStorage.getItem('i18nextLng') || localStorage.getItem('i18nextLng')?.startsWith('az')) {
    localStorage.setItem('i18nextLng', 'en');
    i18n.changeLanguage('en');
  }
}

export default i18n;
