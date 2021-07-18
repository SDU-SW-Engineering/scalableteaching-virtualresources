

function breakStringIntoTokenList(inputString) {
    let userTokens = inputString.split(/[,\s]/g)
    let cleanedTokens = []

    userTokens.forEach(token => {
        let newToken = token.replaceAll(/[,\s]/g, "")
        if (newToken.length > 0)
            cleanedTokens.push(newToken)
    })
    return cleanedTokens
}

export default {
    breakStringIntoTokenList,
}
