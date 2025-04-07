window.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const searchQuery = urlParams.get('Search_Recipe');

    if (searchQuery) {
        Recherche(searchQuery);
    }
});

/* Recherche */



initScriptsfilter();



function initScriptsfilter() {
    // Sélection des éléments
    boutonFilter = document.getElementById('boutonFilter');
    menuFilter = document.getElementById('menuFilter');
    span4 = document.getElementById('span4');
    span5 = document.getElementById('span5');
    span6 = document.getElementById('span6');
    span7 = document.getElementById('span7');
    filter = document.querySelectorAll('.filter');

    // Vérifier immédiatement
    let missingElements = [];
    if (!boutonFilter) missingElements.push("boutonFilter");
    if (!menuFilter) missingElements.push("menuFilter");
    if (!span4) missingElements.push("span4");
    if (!span5) missingElements.push("span5");
    if (!span6) missingElements.push("span6");
    if (!span7) missingElements.push("span7");

    if (missingElements.length > 0) {
        console.warn(`Certains éléments ne sont pas encore prêts: ${missingElements.join(", ")}`);

        // Retenter après 100ms
        setTimeout(initScripts, 100);
        return;
    }

    // Ajout des événements
    boutonFilter.addEventListener('click', toggleMenuFilter);
    filter.forEach(filter => filter.addEventListener('click', toggleMenuFilter));
}


// Fonction pour ouvrir/fermer le menu burger
function toggleMenuFilter() {
    menuFilter.classList.toggle('menuFilterOpen');
    span4.classList.toggle('span4Open');
    span5.classList.toggle('span5Open');
    span6.classList.toggle('span6Open');
    span7.classList.toggle('span7Open');
}




