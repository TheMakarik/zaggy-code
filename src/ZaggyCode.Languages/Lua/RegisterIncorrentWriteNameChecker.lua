
local function levenshtein(s, t)
    local m, n = #s, #t
    local d = {}
    for i = 0, m do d[i] = {} end
    for i = 0, m do d[i][0] = i end
    for j = 0, n do d[0][j] = j end
    for i = 1, m do
        for j = 1, n do
            local cost = (s:sub(i, i) == t:sub(j, j)) and 0 or 1
            d[i][j] = math.min(d[i-1][j] + 1, d[i][j-1] + 1, d[i-1][j-1] + cost)
        end
    end
    return d[m][n]
end

local function find_best_match(tbl, key)
    if type(key) ~= "string" then return nil end
    local best_key, best_dist = nil, math.huge
    for k, _ in pairs(tbl) do
        if type(k) == "string" and k ~= key and not k:match("^__") then
            local dist = levenshtein(key, k)
            if dist < best_dist then
                best_dist = dist
                best_key = k
            end
        end
    end
    if best_dist <= 3 then
        return best_key, best_dist
    end
    return nil
end

local function create_index_func()
    return function(self, key)
        local value = rawget(self, key)
        if value ~= nil then
            return value
        end
        if USE_TABLE_CONTENT_CHECKER then
            local best = find_best_match(self, key)
            if best then
                __clr_throws_IncorrectlyWroteNameException(key, best)
                return nil
            end
        end
        return nil
    end
end

local function apply_metatable(tbl, visited)
    visited = visited or {}
    if visited[tbl] then return end
    visited[tbl] = true
    local old_mt = getmetatable(tbl)
    local new_mt = {}
    if old_mt then
        for k, v in pairs(old_mt) do
            if k ~= "__index" then
                new_mt[k] = v
            end
        end
    end
    new_mt.__index = create_index_func()
    setmetatable(tbl, new_mt)
    for _, v in pairs(tbl) do
        if type(v) == "table" then
            apply_metatable(v, visited)
        end
    end
end

apply_metatable(_G)

local mt_g = getmetatable(_G) or {}
local old_newindex = mt_g.__newindex
mt_g.__newindex = function(tbl, key, value)
    if type(value) == "table" then
        apply_metatable(value)
    end
    rawset(tbl, key, value)
    if old_newindex then
        old_newindex(tbl, key, value)
    end
end
setmetatable(_G, mt_g)
