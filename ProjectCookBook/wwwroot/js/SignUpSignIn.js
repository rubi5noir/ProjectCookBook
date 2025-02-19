let Form = document.getElementById('FormConnexionInscription');
let Bouton_Inscription = document.getElementById('Inscription');
let Bouton_Connexion = document.createElement('button');
let Field_Connexion = document.getElementById('Field_Connexion');
let Field_Inscription = document.createElement('fieldset');

let li;
let div;
let label;
let input;

/* List des For="" des label */
let List_For_Label = ["User_Username_Input_Inscription", "User_Nom_Input_Inscription", "User_Prenom_Input_Inscription", "User_Email_Input_Inscription", "User_Password_Input_Inscription"];
/* List des InnerText des label */
let List_Label = ["Username : ", "Nom : ", "Prénom : ", "Email : ", "Password : "];
/* List des Type="" des input */
let Input_Type_List = ["text", "text", "text", "email", "password", "submit"];
/* List des Name="" des input */
let Input_Name_List = ["User_Username", "User_Nom", "User_Prenom", "User_Email", "User_Password"];
/* List des id="" des input */
let Input_Id_List = ["User_Username_Input_Inscription", "User_Nom_Input_Inscription", "User_Prenom_Input_Inscription", "User_Email_Input_Inscription", "User_Password_Input_Inscription"];



Bouton_Connexion.setAttribute("id", "Bouton_Connexion");
Bouton_Connexion.innerText = "Connexion";
Bouton_Connexion.formNoValidate = true;
Field_Inscription.setAttribute('id', "Field_Inscription");
Fieldset_Inscription();

/* Ajout Event */

Bouton_Inscription.addEventListener('click', ToggleConnexionInscription);
Bouton_Connexion.addEventListener('click', ToggleConnexionInscription);

/* Fonction */

/* label */
function Create_Label(i) {
    label = document.createElement('label');
    label.setAttribute("for", List_For_Label[i]);
    label.innerText = List_Label[i];
    div.appendChild(label);
}

/* input */
function Create_Input(i) {
    input = document.createElement('input');
    if (i >= 5) {
        input.setAttribute("type", Input_Type_List[i]);
        input.setAttribute("value", "Inscription");
    }
    else {
        input.setAttribute("type", Input_Type_List[i]);
        input.setAttribute("name", Input_Name_List[i]);
        input.setAttribute("id", Input_Id_List[i]);
        input.required = true;
    }
    div.appendChild(input);
}

/* Création Fieldset Inscription */
function Fieldset_Inscription() {
    let ul = document.createElement('ul');
    Field_Inscription.appendChild(ul);

    for (let i = 0; i < 7; i++) {

        /* li */
        li = document.createElement('li');
        if (i >= 5) {
            li.classList.add('Submit');
        }
        ul.appendChild(li);

        /* div */
        div = document.createElement('div');
        div.classList.add('Connexion');
        li.appendChild(div);
        if (i >= 5 && i < 6) {
            Create_Input(i);
        }
        else if (i == 6) {
            div.appendChild(Bouton_Connexion);
        }
        else {
            Create_Label(i);
            Create_Input(i);
        }
    }
}

/* Changement Inscription / Connexion */
function ToggleConnexionInscription() {
    if (Form.firstElementChild.id == "Field_Connexion") {
        Form.removeChild(Field_Connexion);
        Form.appendChild(Field_Inscription);
    }
    else {
        Form.removeChild(Field_Inscription);
        Form.appendChild(Field_Connexion);
    }
}