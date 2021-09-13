<template>
  <v-app>
    <v-app-bar
      app
      color="success"
      dark
    >
      <div class="d-flex align-center">
        <v-img
          alt="UniNFe Logo"
          class="shrink mr-2"
          contain
          src='@/assets/uninfe.jpg'
          transition="scale-transition"
          width="40"
        />
        <h2> UniNFe </h2>

        <v-container>
          <v-row class="text-center">
            <v-col cols="4">
              <router-link class="white--text" to="/">Home</router-link>
            </v-col>
            <v-col cols="4">
              <router-link class="white--text" to="/configurations">Configurações</router-link>
            </v-col>
          </v-row>
        </v-container>
      </div>
    </v-app-bar>
  
    <v-container>
      <v-main>
        <router-view/>
      </v-main>
    </v-container>
  </v-app>
</template>

<script>
import { ipcRenderer } from 'electron'
window.ipcRenderer = ipcRenderer

export default {
  name: 'App',
  components: {
  },
  data: () => ({
    //
  }),
  mounted: function(){
    window.ipcRenderer.on('goToHome', ()=>{
      this.$router.push('/');
    });
    window.ipcRenderer.on('goToAbout', ()=>{
      this.$router.push('/about');
    });
  }
};
</script>
