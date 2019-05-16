function GestionVariantes(Str) {

    // Desglosar informacion
    var aInfo = Str.split(' ');    

    //Recolocar variantes en divs
    var multiPv = aInfo.indexOf('multipv');
    var valPv = aInfo[multiPv + 1];
        
    var nVariante = aInfo.indexOf('pv');
    if (nVariante > -1) {
        var cVariante = aInfo[nVariante + 1];
        $('#lbvariante' + valPv).html(cVariante);       
    }     

}