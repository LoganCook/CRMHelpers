// generate an HTML element id from email address
function getIdFromEamil(email: string): string {
    return "id" + email.replace("@", "_").replace(/\./g, "_");
}

// Do an ajax call to Dynamics and display result in the id defined HTML element
function checkCRM(email: string, id: string): void {
    $.ajax("/api/crm/contact?email=" + email).done(function (data) {
        let content = "No";
        if ("contactid" in data) {
            content = "Yes <a href='Entities/Contacts/Edit/" + data["contactid"] + "'>Edit</a>";
        }
        $('#' + id).html(content);
    });
}