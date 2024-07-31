<script lang="ts" setup>
import { useI18n } from 'vue-i18n';

import SubComponet from './SubComponet.vue'

import { supportLanguages, setLanguage } from '@/i18n';
//语言测试
const { t, locale } = useI18n()
//.env配置测试
const env_common = import.meta.env.VITE_COMMON
const env_dev = import.meta.env.VITE_DEV
const env_prod = import.meta.env.VITE_PROD
</script>

<template>
  <div class="home">
    <SubComponet text="11" />
    <img alt="Vue logo" src="../assets/vue.svg"><br>
    <div>
      <h3>.env文件配置测试</h3>
      env_common:{{ env_common }}<br>
      env_dev:{{ env_dev }}<br>
      env_prod:{{ env_prod }}<br>
    </div>
    <div>
      <h3>语言测试</h3>
      {{ t('common.test') }}
      {{ locale }}
      <select>
        <option v-for="lang in supportLanguages" :key="lang" :value="lang" :selected="lang === locale"
          @click="setLanguage(lang)">
          {{ lang }}
        </option>
      </select>
    </div>
  </div>
</template>
<style lang="scss" scoped>
//居中
.home {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  font-size: 20px;
  color: #000;

  select {
    color: #000;
  }

}
</style>