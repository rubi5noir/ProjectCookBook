// Déclaration des variables globales
let boutonBurger = null,
    menuBurger = null,
    span1 = null,
    span2 = null,
    span3 = null,
    elts = [],
    comptebutton = null;

let User_Research_Value;
let Label_Recipes_Search = document.getElementById("Label_Recipes_Search");
let Title_Page_Research = document.getElementById('Title_Page_Research');

// Récupère le thème sauvegardé dans localStorage, ou utilise un thème par défaut (par exemple "Light")
const savedTheme = localStorage.getItem('theme') || 'Light';
// Applique le thème
document.body.className = savedTheme;

console.log("Header chargé, initialisation des scripts...");
console.log("Vérification des éléments : ", document.getElementById('boutonBurger')); // Devrait afficher l'élément
initScripts();


function initScripts() {
    // Sélection des éléments
    boutonBurger = document.getElementById('boutonBurger');
    menuBurger = document.getElementById('menuBurger');
    span1 = document.getElementById('span1');
    span2 = document.getElementById('span2');
    span3 = document.getElementById('span3');
    elts = document.querySelectorAll('.elt');
    comptebutton = document.getElementById("Header_Account_Button");

    // Vérifier immédiatement
    let missingElements = [];
    if (!boutonBurger) missingElements.push("boutonBurger");
    if (!menuBurger) missingElements.push("menuBurger");
    if (!span1) missingElements.push("span1");
    if (!span2) missingElements.push("span2");
    if (!span3) missingElements.push("span3");
    if (!comptebutton) missingElements.push("Header_Account_Button");

    if (missingElements.length > 0) {
        console.warn(`Certains éléments ne sont pas encore prêts: ${missingElements.join(", ")}`);

        // Retenter après 100ms
        setTimeout(initScripts, 100);
        return;
    }

    // Ajout des événements
    boutonBurger.addEventListener('click', toggleMenu);
    comptebutton.addEventListener("click", CompteRedirect);
    elts.forEach(elt => elt.addEventListener('click', toggleMenu));
}


// Fonction pour ouvrir/fermer le menu burger
function toggleMenu() {
    menuBurger.classList.toggle('menuBurgerOpen');
    span1.classList.toggle('span1Open');
    span2.classList.toggle('span2Open');
    span3.classList.toggle('span3Open');
}

// Fonction de redirection vers la page compte
function CompteRedirect() {
    const url = this.getAttribute("data-url");
    if (url) {
        window.location.href = url;
    }
}


/* Recherche */


document.addEventListener("headerLoaded", () => {
    initScriptsfilter();
});


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

function changeTheme(theme) {
    document.body.className = theme; // Change la classe du body
    localStorage.setItem('theme', theme);
};

document.querySelectorAll(".Recipe_Book").forEach(div => {
    div.addEventListener("click", function () {
        const url = this.getAttribute("data-url");
        if (url) {
            window.location.href = url;
        }
    })
});

document.querySelectorAll(".Compte_Vignette").forEach(div => {
    div.addEventListener("click", function () {
        const url = this.getAttribute("data-url");
        if (url) {
            window.location.href = url;
        }
    })
});

function ClickOnCategorie(event) {
    let h2Element;
    if (event.target.tagName === "H2") {
        h2Element = event.target;
    }
    else {
        h2Element = event.target.querySelector('h2');
    }
    let User_Research = h2Element.innerText;
    localStorage.setItem('userResearch', "Recherche : " + User_Research);
    window.location.href = '/Main/Recipes/Recipe_Search.html';
}