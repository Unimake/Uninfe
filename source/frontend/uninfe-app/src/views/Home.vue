<!--

<template>
  <div class="home">
    <HelloWorld msg="Welcome to Your Vue.js App"/>
  </div>
</template>

<script>
// @ is an alias to /src
import HelloWorld from '@/components/HelloWorld.vue'
export default {
  name: 'home',
  components: {
    HelloWorld
  }
}
</script>

-->


<template>
  <div>

    <v-img
      max-height="150"
      max-width="150"
      src='@/assets/uninfe.jpg'
    ></v-img>

    <p style="font-size:20px;"> Monitor DF-e </p>
    
    <v-btn href="www.unimake.com.br" target="_blank"
      text
      color="success"
    >
      www.unimake.com.br
    </v-btn>

  </div>
</template>