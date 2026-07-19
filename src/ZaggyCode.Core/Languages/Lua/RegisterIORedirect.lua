
io.read = function()
    return __clr_input:ReadLine()
end

io.write = function(text)
    __clr_output:Write(tostring(text))
end

print = function(text)
    __clr_output:WriteLine(tostring(text))
end

__debug("IO Was redirected")