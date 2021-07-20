

function breakStringIntoTokenList(inputString) {
    let userTokens = inputString.split(/[,\s]/g)
    let cleanedTokens = []

    for(let k = 0; k < userTokens.length; k++){
        let token = userTokens[k]
        let newToken = token.replaceAll(/[,\s]/g, "")
        if (newToken.length > 0)
            cleanedTokens.push(newToken)
    }
    return cleanedTokens
}

export default {
    breakStringIntoTokenList,
}
