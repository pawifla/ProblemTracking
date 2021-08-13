$(document).ready(function () {
    
});

let darkMode = localStorage.getItem("darkMode");

let themeLabel = $('#btn_themeToggle').val();
//TODO:: make owner icon white here.

const enableDarkMode = () => {
    $('#theme').attr('href', '../Content/bootstrap-dark.min.css');
    $('#theme2').attr('href', 'CSS/DarkDatatable.css');
    $('#btn_themeToggle').html('<img id ="darkModeIcon" src="/Images/toggle-off.svg" />');
    $('#btn_themeToggle').attr('class', 'btn btn-light btn-sm');
    $('#loader').html('<img  src="Images/smallerLoader.gif"/>');
    $('.owner_icon').html('<img src="/Images/person-lines-fill-white.svg" />');
    localStorage.setItem("darkMode", 'enabled');
}
const disableDarkMode = () => {
    $('#theme').attr('href', '../Content/bootstrap.min.css');
    $('#theme2').attr('href', '');
    $('#btn_themeToggle').html('<img id ="darkModeIcon" src="/Images/toggle-on.svg" />');
    $('#btn_themeToggle').attr('class', 'btn btn-dark btn-sm');
    $('#loader').html('<img  src="Images/smallerLoader-white.gif"/>');
    $('.owner_icon').html('<img src="/Images/person-lines-fill.svg" />');
    localStorage.setItem("darkMode", null);
}

//populate owner icon when table created
function checkTheme() {
    console.log(darkMode);
    if (darkMode === 'enabled') {
        enableDarkMode();
    }
    else {
        disableDarkMode();
    }
};

if (darkMode === 'enabled') {
    enableDarkMode();
}
$('#btn_themeToggle').click(function () {
    darkMode = localStorage.getItem("darkMode");
    if (darkMode !== 'enabled') {
        enableDarkMode();
    }
    else {
        disableDarkMode();
    }
    location.reload();

});
