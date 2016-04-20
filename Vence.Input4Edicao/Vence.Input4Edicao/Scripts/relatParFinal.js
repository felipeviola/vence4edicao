var global = {

    init: function () {
        this.config();
        this.limparCampos();
        eventos.onClick();
        global.formValidation();
    },

    params: {
        $Chave: null,
        $SomenteParcelasPlanoContigencia: null,
        $btnPesquisa: null,
        $btnImprimir: null,
        $frmPesquisa: null
    },

    config: function () {
        this.params.$Chave = $("#Chave");
        this.params.$SomenteParcelasPlanoContigencia = $("#SomenteParcelasPlanoContigencia");
        this.params.$btnPesquisa = $("#btnPesquisa");
        this.params.$frmPesquisa = $("#frmPesquisa");
    },

    formValidation: function () {
        $("#frmPesquisa").validate({
            rules: {
                Chave: { required: true }
            },
            messages: {
                Chave: { required: 'Obrigatório!' }
            }
        });
    },

    limparCampos: function () {
        global.params.$Chave.val('');
    },

    mensageSucess: function (message) {
        alert(message);
    },

    mensageError: function (message) {
        alert(message);
    },

    mensageWarning: function (message) {
        alert(message);
    },

    showAjaxLoader: function () {
        $("#ajaxLoading").removeClass("imgHidde");
        $("#ajaxLoading").addClass("imgVisible");
    },

    hideAjaxLoader: function () {
        $("#ajaxLoading").removeClass("imgVisible");
        $("#ajaxLoading").addClass("imgHidde");
    }
}

var eventos = {

    onClick: function () {

        global.params.$btnPesquisa.on('click', function () {
            $('#gridView').html('');
            if (global.params.$frmPesquisa.valid() === true) {
                eventos.pesquisar();
            }
        });
    },    
    
    pesquisar: function () {

        global.showAjaxLoader();

        var chave = global.params.$Chave.val();
        var somentePlanoContigencia = global.params.$SomenteParcelasPlanoContigencia.is(":checked");

        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'html',
            url: '../RelatParcelaFinal/Pesquisar',
            data: { chave: chave, somentePlanoContigencia: somentePlanoContigencia },
            success: function (data) {
                if (data != undefined && data.length > 0) {
                    $('#gridView').html(data);
                }
                global.hideAjaxLoader();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $(document).ajaxStop(function () {
                    global.mensageError('Ocorreu um erro durante o processo.');
                    global.hideAjaxLoader();
                });
            }
        });
    }
}

$(document).ready(function () {
    global.init();
});