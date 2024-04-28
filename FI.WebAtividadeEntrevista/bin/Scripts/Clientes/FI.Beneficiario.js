$(document).ready(function () {
    $('#addBeneficiario').click(function () {
        var name = $("#nameBeneficiario").val();
        var cpf = $("#CPFBeneficiario").val();
        //---->Form validation goes here
        var new_row = '\
        <tr>\
            <td class="cpfBeneficiario">' + cpf + '</td>\
            <td class="nameBeneficiario">' + name + '</td>\
            <td class="text-center">\
                <button type="button" title="Remover Beneficiário" class="btn btn-danger btn-sm remove-image">\
                    Remover\
                </button>\
            </td>\
        </tr>';
        $("#beneficiariosTable").append(new_row);

        $("#nameBeneficiario").val("");
        $("#CPFBeneficiario").val("");
    });

    $('#beneficiariosTable').on('click', '.remove-image', function () {
        var row = $(this).closest('tr');

        if (row != null) {
            row.remove();
        }
    });
})
