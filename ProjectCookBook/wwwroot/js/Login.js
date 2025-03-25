let FormInscription = document.getElementById('FormInscription');
let FormConnexion = document.getElementById('FormConnexion');

let Field_Connexion = document.getElementById('Field_Connexion');
let Field_Inscription = document.getElementById('Field_Inscription');

let Bouton_Inscription = document.getElementById('Inscription');
let Bouton_Connexion = document.getElementById('Connexion');

/* Ajout Event */

Bouton_Inscription.addEventListener('click', ToggleConnexionInscription);
Bouton_Connexion.addEventListener('click', ToggleConnexionInscription);

/* hidding of the Inscription fieldset */

Field_Inscription.remove();

let isConnexionVisible = true;

/* Ajout des events */
Bouton_Inscription.addEventListener("click", ToggleConnexionInscription);
Bouton_Connexion.addEventListener("click", ToggleConnexionInscription);

/* Changement Inscription / Connexion */
function ToggleConnexionInscription() {
    if (isConnexionVisible) {
        Field_Connexion.remove();
        FormInscription.appendChild(Field_Inscription);
    } else {
        Field_Inscription.remove();
        FormConnexion.appendChild(Field_Connexion);
    }
    isConnexionVisible = !isConnexionVisible;
};