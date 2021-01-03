export default {
    parseResponse
}

function parseResponse(response) {
    if (response.ok) {
        return Promise.resolve(response.json())
            .catch()
    } else {
        return response.json().then(json => {
            if (json.errors) {
                const firstKey = Object.keys(json.errors)[0]
                const firstErrorArray = json.errors[firstKey];
                const firstError = firstErrorArray[0]
                return Promise.reject(new Error(firstError))
            } else {
                return Promise.reject(new Error(json))
            }
        })
    }
}