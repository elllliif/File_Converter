// Nova PDF Configuration
const CONFIG = {
    // API_BASE: "http://localhost:5000/api",
    API_BASE: "http://localhost:5000/api" // Local Dev
    // Change this to your production domain when deploying, e.g., "https://api.novapdf.com/api"
};

if (typeof module !== 'undefined' && module.exports) {
    module.exports = CONFIG;
}
