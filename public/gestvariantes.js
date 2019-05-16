function GestionVariantes(Str) {

    // Desglosar informacion
    var aInfo = Str.split(' ');

    // Mostrar profundidad
    var nDepth = aInfo.indexOf('depth');
    if (nDepth > -1) {
        var cDepth = aInfo[nDepth + 1];
        $('#profundidad').text(cDepth);
    }

    // Mostrar lineas multipv
    var NumeVari = $('#multyPv').val();
    NumeVari = parseInt(NumeVari);
    var ExtraDiv = 54;
    var TotalHeightDiv = ExtraDiv * NumeVari;
    $('#variantes').empty();

    //Crear divs variantes
    for (var i = 1; i < NumeVari + 1; i++) {
        $('#variantes').append('<div id="variante"' + NumeVari.toString() + ' style="height:40px; width:680px; border:2px solid black; margin-top:8px; margin-left:8px;">' +
            '<label id="variante' + NumeVari.toString() + '">Aki va la variante</label>' +
            '</div > ');
        $('#variantes').css('height', TotalHeightDiv + 'px');
    }



}