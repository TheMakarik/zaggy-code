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

local function find_best_match(table_to_search, key_to_find, visited_keys)
    if type(key_to_find) ~= "string" then return nil end
    visited_keys = visited_keys or {}
    local best_key, best_distance = nil, math.huge
    local current_key, current_value = next(table_to_search, nil)
    while current_key ~= nil do
        if type(current_key) == "string" and current_key ~= key_to_find and not current_key:match("^__") and not visited_keys[current_key] then
            visited_keys[current_key] = true
            local distance = levenshtein(key_to_find, current_key)
            if distance < best_distance then
                best_distance = distance
                best_key = current_key
            end
        end
        current_key, current_value = next(table_to_search, current_key)
    end
    if best_distance <= 3 then
        return best_key, best_distance
    end
    return nil
end

local function create_index_function()
    return function(self, key)
        local value = rawget(self, key)
        if value ~= nil then
            return value
        end
        if rawget(_G, "USE_TABLE_CONTENT_CHECKER") then
            local best = find_best_match(self, key, {})
            if best then
                __clr_throws_IncorrectlyWroteNameException(key, best)
                return nil
            end
        end
        return nil
    end
end

local function should_process_table(table_to_check)
    local meta_table = getmetatable(table_to_check)
    if meta_table then
        local meta_table_name = rawget(meta_table, "__name")
        if meta_table_name and type(meta_table_name) == "string" and meta_table_name:match("^__clr") then
            return false
        end
    end
    return true
end

local function apply_metatable(table_to_process, visited_tables, current_depth)
    current_depth = current_depth or 0
    if current_depth > 100 then return end

    visited_tables = visited_tables or {}
    if visited_tables[table_to_process] then return end
    visited_tables[table_to_process] = true

    if not should_process_table(table_to_process) then
        return
    end

    local old_meta_table = getmetatable(table_to_process)
    local new_meta_table = {}
    if old_meta_table then
        local current_key, current_value = next(old_meta_table, nil)
        while current_key ~= nil do
            if current_key ~= "__index" and type(current_key) ~= "table" then
                new_meta_table[current_key] = current_value
            end
            current_key, current_value = next(old_meta_table, current_key)
        end
    end
    new_meta_table.__index = create_index_function()
    setmetatable(table_to_process, new_meta_table)

    local current_key, current_value = next(table_to_process, nil)
    while current_key ~= nil do
        if type(current_key) ~= "string" or not current_key:match("^__clr") then
            if type(current_value) == "table" then
                if should_process_table(current_value) and not visited_tables[current_value] then
                    apply_metatable(current_value, visited_tables, current_depth + 1)
                end
            end
        end
        current_key, current_value = next(table_to_process, current_key)
    end
end

apply_metatable(_G)

local global_meta_table = getmetatable(_G) or {}
local old_new_index = global_meta_table.__newindex
global_meta_table.__newindex = function(table_to_modify, key, value)
    if type(key) == "string" and key:match("^__clr") then
        rawset(table_to_modify, key, value)
        if old_new_index then
            old_new_index(table_to_modify, key, value)
        end
        return
    end

    if type(value) == "table" then
        if should_process_table(value) then
            apply_metatable(value, {}, 0)
        end
    end
    rawset(table_to_modify, key, value)
    if old_new_index then
        old_new_index(table_to_modify, key, value)
    end
end
setmetatable(_G, global_meta_table)