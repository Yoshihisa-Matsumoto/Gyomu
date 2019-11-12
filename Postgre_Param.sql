
DROP TABLE if exists param_master;

CREATE TABLE param_master(
	item_key varchar(50) NOT NULL,
	item_value text NOT NULL,
	item_fromdate varchar(8) NOT NULL default '',
 PRIMARY KEY (item_key,	item_fromdate )
);