function getIdFromEamil(email) {
    return "id" + email.replace("@", "_").replace(/\./g, "_");
}
function checkCRM(email, id) {
    $.ajax("/api/crm/contact?email=" + email).done(function (data) {
        var content = "No";
        if ("contactid" in data) {
            content = "Yes <a href='Entities/Contacts/Edit/" + data["contactid"] + "'>Edit</a>";
        }
        $('#' + id).html(content);
    });
}
//# sourceMappingURL=indexpage.js.map