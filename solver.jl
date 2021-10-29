words = read("./words.txt", String)
max_len = 7
min_len = 3

#compare each char with the original string
#if i've seen it before, it's a dupe, return immediately
function has_duplicates(s::String)
    seen = []
    repeat = false
    for c in s 
        if !in(c, seen)
            push!(seen, c)
        else
            repeat = true
            break
        end
    end
    return repeat
end

#compare each char in string with string
#if i've seen it before, it's a dupe track it
function find_duplicate_chars(s::String)
    seen = []
    dupes = []
    for c in s 
        if !in(c, seen)
            push!(seen, c)
        else
            if !in(c, dupes)
                push!(dupes, c)
            end
        end
    end
    return dupes
end


#compare the dupes to a list of dupes, 
#if i'm not in the list of dupes, but i'm a dupe
#this violates the constraint from the game
#impossible to construct with duplicate char which is not given
#ex ; set = a,c,e,e,r,f ; word = facerr, impossible given current character set  
function duplicates_match_charset(s::String, charset::AbstractArray)
    state = true
    for c in find_duplicate_chars(s)
        if !in(string(c), charset)
            state = false
            break
        end
    end
    return state
end

function solve(s::String, repeats::Bool, repeatingset::AbstractArray)
    matches = []
    for word = split(words, "\n")
        word = lowercase(word)
        possible = true
        constrained = length(word) >= min_len && length(word) <= max_len
        #our word contains a character which is not in the provided character set
        #impossible to construct, skip
        for char = word
            if !occursin(string(char), s)
                possible = false
                break
            end
        end
        #only contains characters from the set, possible to construct
        if ( possible == true ) && constrained
            #no repeated characters in given set, check word for dupes
            if repeats == false
                if has_duplicates(string(word)) == true
                    continue
                else
                    append!(matches, [word])
                end
            #repeated chars in given set
            else
                #prune words to just those which repeat on the given repeated characters in set
                if duplicates_match_charset(string(word), repeatingset) == true
                    append!(matches, [word])
                else
                   continue
                end
            end
        end
    end
    return matches
end

function print_res(xs::AbstractArray)
    println("------------")
    for w in sort!(sort!(xs), by = x -> length(x))
        println(w)
    end
end

function go()
    println("Enter Words:")
    input = readline()

    println("Has repeating characters?")
    if in(readline(), ["true","yes"])
        println("Enter repeating characters:")
        print_res(solve(input, true, split(readline(), "")))
    else
        print_res(solve(input, false, []))
    end

    println("Continue?")
    #terminal condition
    if !in(readline(), ["no","false"])
        go()
    end
end
go()