const app = Vue.createApp({
    data() {
        return {
            data: null,
            prompt: null,
            apiResponse: null,
            keys: null,
            isLoading: false
        }
    },
    methods: {
        initialState() {
            this.data = null
        },
        initialStateTotal() {
            this.data = null,
                this.prompt = null,
                this.apiResponse = null,
                this.keys = null
        },
        scrollToHero() {
            const heroElement = document.getElementById("hero");
            heroElement.scrollIntoView({ behavior: "smooth" });
        },
        scrollToMarketing() {
            const marketingElement = document.getElementById("marketing");
            marketingElement.scrollIntoView({ behavior: "smooth" });
        },
        askDatabase() {
            // Mostrar el indicador de carga
            this.isLoading = true;

            // Simple POST request with a JSON body using fetch
            const requestOptions = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ prompt: this.prompt })
            };
            fetch("https://aitosql.azurewebsites.net/AskTheDatabase", requestOptions) // https://localhost:7006/AskTheDatabase // https://aitosql.azurewebsites.net/AskTheDatabase
                .then(response => response.json())
                .then(data => {
                    console.log(data);
                    this.apiResponse = data;
                    if (data && data.length > 0) {
                        this.keys = Object.keys(data[0]);
                    }
                    this.scrollToMarketing();

                    // Ocultar el indicador de carga
                    this.isLoading = false;
                })
                .catch(error => {
                    console.error("Error en la petición:", error);

                    // Ocultar el indicador de carga
                    this.isLoading = false;
                });
        }
    }
});

app.mount("#vueApp");