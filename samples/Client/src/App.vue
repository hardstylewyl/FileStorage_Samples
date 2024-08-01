<script setup lang="ts">

</script>

<template>
  <VApp>
    <VMain>
      <RouterView>
        <template #default="{ Component }">
          <Transition>
            <Component :is="Component" />
          </Transition>
        </template>
      </RouterView>
    </VMain>
  </VApp>
</template>
