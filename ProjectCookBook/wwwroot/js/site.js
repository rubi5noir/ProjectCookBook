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











/* fetch */

let boutonSubmitInputRecherche = document.getElementById("Search_Recipe_Button");

boutonSubmitInputRecherche.addEventListener('click', Recherche);

let Body_Recipes = document.getElementById('Body_Recipes');

function Recherche(param) {
    // récupération de la valeur de la recherche
    let inputRecherche = document.getElementById("Search_Recipe");
    let chaine = inputRecherche.value;
    let hiddenInputRecherche = document.getElementById("recherche");

    // Cas 1 : appelé par un clic → param est un MouseEvent
    // Cas 2 : appelé avec une string (via script) → param est la chaîne recherchée
    if (param instanceof Event) {
        chaine = inputRecherche.value;
    } else {
        chaine = param || "";
        if (inputRecherche) inputRecherche.value = chaine;
    }

    if (!chaine && hiddenInputRecherche) {
        chaine = hiddenInputRecherche.value;
    }

    if (hiddenInputRecherche) hiddenInputRecherche.value = chaine;

    if (window.location.pathname !== "/Recettes/PageSearch") {
        window.location.href = "/Recettes/PageSearch?Search_Recipe=" + encodeURIComponent(chaine);
        return;
    }

    Body_Recipes = document.getElementById('Body_Recipes');
    fetch("/Recettes/Search?Search_Recipe=" + encodeURIComponent(chaine)).then((reponse) => {
        return reponse.json();
    }).then((json) => {
        console.log(json);
        Body_Recipes.innerHTML = ''; // Vide le contenu précédent
        for (let i = 0; i < json.recettes.length; i++) {
            afficherRecette(json.recettes[i]);
        }
    });
}



function afficherRecette(recette) {

    let Recipe_Book = document.createElement('div');
    let div = document.createElement('div');
    let h3_note = document.createElement('h3');
    let h3_Star = document.createElement('h3');
    let img = document.createElement('img');
    let h2 = document.createElement('h2');

    Recipe_Book.classList.add('Recipe_Book');
    Recipe_Book.setAttribute("data-url", `/Recettes/Detail/${recette.id}`);
    Recipe_Book.addEventListener('click', () => {
        window.location.href = `/Recettes/Detail/${recette.id}`;
    });

    Recipe_Book.appendChild(div);

    h3_Star.classList.add('Rating');

    div.appendChild(h3_Star);

    h3_note.classList.add('Rating_On_Vignette');
    h3_note.innerText = recette.avisnote;

    div.appendChild(h3_note);

    img.src = recette.img;

    div.appendChild(img);

    h2.innerText = recette.nom;

    div.appendChild(h2);

    Body_Recipes.appendChild(Recipe_Book);
}