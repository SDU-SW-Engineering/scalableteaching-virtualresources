export default {
    parseResponse
};

async function parseResponse(response) {

    if (response.ok) {
        let json = await response.json();
        if(process.env.NODE_ENV === 'development')
            window.console.log("ResponseJson: ", json)
        return json
    } else {
        return response.json().then(json => {
            if (json.errors) {
                const firstKey = Object.keys(json.errors)[0];
                const firstErrorArray = json.errors[firstKey];
                const firstError = firstErrorArray[0];
                return Promise.reject(new Error(firstError));
            } else {
                return Promise.reject(new Error(json));
            }
        });
    }
}