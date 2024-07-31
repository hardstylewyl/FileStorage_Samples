<script setup lang="ts">

</script>

<template>
  <RouterView>
    <template #default="{ Component }">
      <Transition>
        <Component :is="Component" />
      </Transition>
    </template>
  </RouterView>
</template>
