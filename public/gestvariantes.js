function GestionVariantes(Str) {

    // Completed variant
    var flag = true;

    // Desglosar informacion
    var aInfo = Str.split(' ');    

    //Recolocar variantes en divs
    var multiPv = aInfo.indexOf('multipv');
    var valPv = aInfo[multiPv + 1];
        
    // Calcular profundidad
    var cVariante = '';
    var nDepth = aInfo.indexOf('depth');
    if (nDepth > -1) {
        nDepth = aInfo[nDepth + 1];
        // Mostrar variantes
        var nVariante = aInfo.indexOf('pv');
        if (nVariante > -1) {
            for (var i = 0; i < nDepth; i++) {
                if (aInfo[nVariante + 1 + i] === undefined) {
                    flag = false;
                }
                cVariante = cVariante + ' ' + aInfo[nVariante + 1 + i];
            }
            if (flag) {
                $('#lbvariante' + valPv).html(cVariante);
            }           
        }
    }   

    // Restore at the end
    if (aInfo[0] == 'bestmove') {        
        $('#calcular').show();
        $('#LabelCalculando').hide();
        $('#loader').hide();
    }

}