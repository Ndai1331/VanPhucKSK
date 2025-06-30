/*
Template Name: CoreAdminWeb - Admin & Dashboard Template
Author: SRBThemes
Version: 2.3.0
File: editors init Js File
*/

function app() {
    return {
        coreAdminWeb: null,
        init: function (el) {
            // Get el
            this.coreAdminWeb = el;
            // Add CSS
            if (this.coreAdminWeb) {
                this.coreAdminWeb.contentDocument.querySelector(
                    "head"
                ).innerHTML += `<style>
            *, ::after, ::before {box-sizing: border-box;}
            :root {tab-size: 4;}
            html {line-height: 1;text-size-adjust: 100%;}
            body {margin: 0px; padding: 1rem 0.5rem;}
            body {font-family: Cerebri Sans;}
            p {margin: 0px;line-height:1.2;}
            </style>`;
                this.coreAdminWeb.contentDocument.body.innerHTML += `
            <h1>Hello World!</h1>
            <p>Welcome to the pure AlpineJS and Tailwind coreAdminWeb.</p>
            `;
                // Make editable
                this.coreAdminWeb.contentDocument.designMode = "on";
            }
        },
        format: function (cmd, param) {
            if (this.coreAdminWeb) {
                this.coreAdminWeb.contentDocument.execCommand(cmd, !1, param || null);
            }
        },
    };
}
