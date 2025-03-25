
/* Mise à jour du temps total */
function updateTotalTime() {
    const cookTime_Heures = parseInt(document.getElementById("Cooking_Time_Cook_Heures").value) || 0;
    const heatingTime_Heures = parseInt(document.getElementById("Cooking_Time_Heating_Heures").value) || 0;
    const cookTime_Heure_Minutes = parseInt(document.getElementById("Cooking_Time_Cook_Minutes").value) || 0;
    const heatingTime_Minutes = parseInt(document.getElementById("Cooking_Time_Heating_Minutes").value) || 0;
    var totalTime_Heures = cookTime_Heures + heatingTime_Heures;
    var totalTime_Minutes = cookTime_Heure_Minutes + heatingTime_Minutes;
    if (totalTime_Minutes >= 60)
    {
        totalTime_Heures = totalTime_Heures + 1;
        totalTime_Minutes = (cookTime_Heure_Minutes + heatingTime_Minutes) % 60;
    }
    const totalTime = totalTime_Heures + "h " + totalTime_Minutes + "mins";

    document.getElementById("totalTime").textContent = totalTime;
}

updateTotalTime();

