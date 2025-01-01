﻿function Like(BroadcastId) {
    const url = '/Home/Like';

    const options = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            PostId: BroadcastId
        }),
    };

    fetch(url, options)
        .then(response => {
            if (response.ok) {
                return response.json(); // Assuming the server returns the updated like count and like state.
            } else {
                console.error("Failed to update like.");
            }
        })
        .then(data => {
            if (data) {
                // Find the like button and count for the corresponding BroadcastId
                const likeButton = document.querySelector(`#like-button-${BroadcastId}`);
                const likeCount = document.querySelector(`#like-count-${BroadcastId}`);

                // Update the button/icon state
                if (data.isLiked) {
                    likeButton.innerHTML = `<svg width="20px" height="20px" viewBox="0 0 1024 1024" class="icon" version="1.1" xmlns="http://www.w3.org/2000/svg"><path d="M895.36 243.904a251.52 251.52 0 0 0-355.776 0l-20.096 20.096-20.096-20.096A251.52 251.52 0 0 0 143.616 599.68S466.24 926.72 512 928c14.336 0.384 86.4-59.52 164.224-128.192l-0.512-0.64a22.016 22.016 0 0 0-11.968-40.896 21.76 21.76 0 0 0-14.784 5.888l-0.064-0.064 62.336-56.832a22.08 22.08 0 0 0-7.808 16.704 22.4 22.4 0 0 0 22.4 22.464c5.44 0 10.24-2.176 14.208-5.44l0.256 0.32 50.048-45.76-0.448-0.448a22.08 22.08 0 0 0-16.768-36.992 21.952 21.952 0 0 0-14.656 5.824l80.384-73.472 0.512 0.512a22.08 22.08 0 0 0-5.696 14.592 22.4 22.4 0 0 0 22.4 22.464 22.016 22.016 0 0 0 14.272-5.504l0.32 0.384 24.832-23.168a251.776 251.776 0 0 0-0.128-355.84z" fill="" /><path d="M510.976 878.656c-51.008-33.344-207.168-180.416-335.488-310.528a206.976 206.976 0 0 1-0.192-292.544c39.04-39.104 91.008-60.608 146.24-60.608s107.136 21.504 146.176 60.544l51.84 51.84 51.84-51.776c39.04-39.04 90.944-60.544 146.176-60.544s107.2 21.504 146.176 60.544c39.04 39.04 60.544 90.944 60.544 146.24s-21.504 107.136-60.544 146.176c-140.096 131.776-301.76 276.032-352.768 310.656z" fill="#FF5F5F" /><path d="M308.032 641.984a15.872 15.872 0 0 1-10.112-3.648 757.12 757.12 0 0 1-53.504-48.896 875.968 875.968 0 0 0-25.856-24.64C141.376 495.488 145.344 423.616 145.536 420.544 143.808 318.976 237.376 264.64 241.344 262.4a16 16 0 0 1 15.808 27.84c-0.832 0.448-81.088 47.488-79.744 131.2-0.064 3.648-2.368 61.248 62.528 119.552 8.704 7.808 17.536 16.448 26.816 25.536 15.616 15.36 31.808 31.168 51.328 47.104a15.936 15.936 0 1 1-10.048 28.352zM422.656 751.36a15.872 15.872 0 0 1-11.2-4.544l-61.312-60.032a16 16 0 1 1 22.4-22.912l61.312 60.032a16 16 0 0 1-11.2 27.456z" fill="#FFFFFF" /></svg>
`                   ; // Filled heart icon
                } else {
                    likeButton.innerHTML = `<svg version="1.1" id="Uploaded to svgrepo.com" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"
                                     width="20px" height="20px" viewBox="0 0 32 32" xml:space="preserve">
                                <style type="text/css">
                                    .linesandangles_een {
                                        fill: #111918;
                                    }
                                </style>
                                <path class="linesandangles_een" d="M10.5,8v2C9.122,10,8,11.121,8,12.5H6C6,10.019,8.019,8,10.5,8z" />
                                <path class="linesandangles_een" d="M21.5,5c-2.116,0-4.093,0.881-5.5,2.406C14.593,5.881,12.616,5,10.5,5C6.364,5,3,8.333,3,12.5
	                            C3,21.542,16,27,16,27s13-5.458,13-14.5C29,8.333,25.636,5,21.5,5z M16,24.797C13.378,23.521,5,18.938,5,12.5
	                            C5,9.467,7.467,7,10.5,7c1.55,0,2.982,0.626,4.03,1.762l0.735,0.797h1.47l0.735-0.797C18.518,7.626,19.95,7,21.5,7
	                            c3.033,0,5.5,2.467,5.5,5.5C27,18.938,18.622,23.521,16,24.797z" /></svg>`; // Empty heart icon
                }

                // Update the like count
                likeCount.textContent = `(${data.likeCount})`;
            }
        })
        .catch(error => {
            console.error("Error:", error);
        });
}
