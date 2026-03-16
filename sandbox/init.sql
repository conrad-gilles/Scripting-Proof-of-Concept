-- Main script table
CREATE TABLE customer_scripts (
    id UUID PRIMARY KEY,
    script_name VARCHAR(100),
    script_type VARCHAR(50),    -- 'GeneratorCondition', 'GeneratorAction'
    source_code TEXT NOT NULL,
    min_api_version INT NOT NULL,
    -- From [ScriptMetadata] attribute
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100)
    -- PRIMARY KEY (script_name,script_type)
);
-- Compiled versions per API
CREATE TABLE script_compiled_cache (
    script_id UUID REFERENCES customer_scripts(id) ON DELETE CASCADE,
    -- script_name VARCHAR(100),
    api_version INT,
    assembly_bytes BYTEA,
    --change this to allow null so you can save even if failed, then obviously dont save the assembly bytes
    compilation_date TIMESTAMP NOT NULL,
    compilation_success BOOLEAN NOT NULL,
    compilation_errors TEXT,
    -- JSON array if failed
    old_source_code TEXT,
    PRIMARY KEY (script_id, api_version)
);
-- Track active Ember instances
CREATE TABLE ember_instances (
    instance_id UUID PRIMARY KEY,
    instance_name VARCHAR(100),
    ember_version VARCHAR(20) NOT NULL,
    -- "2.3.0"
    sdk_version INT NOT NULL,
    -- 2
    last_heartbeat TIMESTAMP NOT NULL,
    hostname VARCHAR(255)
);
-- Index for cleanup
CREATE INDEX idx_ember_instances_heartbeat ON ember_instances(last_heartbeat);
