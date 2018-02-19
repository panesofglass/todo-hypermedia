import _ from './collection';
import { expect } from 'chai';
import SemanticLink from '../SemanticLink';

describe('Collection mixins', () => {

    let document = {
        links: [{
            rel: 'self', href: 'http://example.com/role/1'
        }],
        name: 'Admin'
    };

    let collection = {
        links: [
            {rel: 'self', href: 'http://example.com/role/'}
        ],
        items: [document]
    };

    describe('_.differenceByUriOrName', () => {

        describe('Collection', () => {

            it('should return empty if sets both empty', () => {
                expect(_({}).differenceByUriOrName({})).to.deep.equal([]);
            });

            it('should return empty if sets are the same', () => {
                expect(_(collection).differenceByUriOrName(collection)).to.deep.equal([]);
            });

            describe('different collection', () => {

                let document2 = {
                    links: [{
                        rel: 'self', href: 'http://example.com/role/2'
                    }],
                    name: 'NOtAdmin'
                };

                let collection1 = {
                    items: [document]
                };
                let collection2 = {
                    items: [document, document2]
                };

                it('should return missing document', () => {
                    expect(_(collection2).differenceByUriOrName(collection1)).to.deep.equal([document2]);
                });

                it('should return empty if sets are the same', () => {
                    expect(_(collection1).differenceByUriOrName(collection2)).to.deep.equal([]);
                });

            });
        });

        describe('LinkedRepresentation[]', () => {

            it('should return empty if set are both empty', () => {
                expect(_([]).differenceByUriOrName()).to.deep.equal([]);
            });

            it('should return empty if sets are the same', () => {
                expect(_([document]).differenceByUriOrName([document])).to.deep.equal([]);
            });

            describe('different collection', () => {

                let document2 = {
                    links: [{
                        rel: 'self', href: 'http://example.com/role/2'
                    }],
                    name: 'NOtAdmin'
                };

                let collection1 = [document];
                let collection2 = [document, document2];

                it('should return missing document', () => {
                    expect(_(collection2).differenceByUriOrName(collection1)).to.deep.equal([document2]);
                });

                it('should return empty if sets are the same', () => {
                    expect(_(collection1).differenceByUriOrName(collection2)).to.deep.equal([]);
                });

            });

        });

    });

    describe('_.pushResource', () => {

        beforeEach(() => {
            document = {
                links: [{
                    rel: 'self', href: 'http://example.com/role/1'
                }],
                name: 'Admin'
            };
        });

        let document2 = {
            links: [{
                rel: 'self', href: 'http://example.com/role/1'
            }],
            name: 'Admin'
        };
        let document3 = {
            links: [{
                rel: 'self', href: 'http://example.com/role/2'
            }],
            name: 'Admin2'
        };

        it('should push onto empty array', () => {
            const result = _([]).pushResource(document2);
            expect(result).to.deep.equal([document2]);
        });

        it('should not push when resource already exists', () => {
            const result = _([document]).pushResource(document2);
            expect(result).to.deep.equal([document]);
        });

        it('should push if new', () => {
            const result = _([document]).pushResource(document2);
            const result2 = _(result).pushResource(document3);
            expect(result2).to.deep.equal([document, document3]);
        });
    });

    describe('_.findItemByUriOrName', () => {
        it('document with uri', () => {
            let found = _(collection).findItemByUriOrName('http://example.com/role/1');
            expect(found).to.deep.equal(document);
        });

        it('document with name as default', () => {
            let found = _(collection).findItemByUriOrName('Admin');
            expect(found).to.deep.equal(document);
        });

        it('document with name', () => {
            let found = _(collection).findItemByUriOrName('Admin', 'name');
            expect(found).to.deep.equal(document);
        });

        it('document with name', () => {
            let found = _(collection).findItemByUriOrName('Admin', 'title');
            expect(found).not.to.deep.equal(document);
        });

        it('document with default attribute title returns not found as undefined', () => {
            let notFound = _(collection).findItemByUriOrName('http://example.com/role/1', /up/);
            expect(notFound).to.be.undefined;
        });
    });

    describe('_.findResourceInCollection', () => {

        it('document with self and name returns item in collection', () => {
            let found = _(collection).findResourceInCollection(document);
            expect(found).to.deep.equal(document);
        });

        it('document with self and without name returns item in collection', () => {
            let resource = {
                links: [{
                    rel: 'self', href: 'http://example.com/role/1'
                }]
            };

            let found = _(collection).findResourceInCollection(resource);
            expect(found).to.deep.equal(document);
        });

        it('document without self and with name returns item from collection and not resource as search input', () => {
            let resource = {
                links: [{
                    rel: 'self', href: 'http://example.com/role/2'
                }],
                name: 'Admin'
            };

            let found = _(collection).findResourceInCollection(resource, 'name');
            expect(found).to.deep.equal(document);
            expect(SemanticLink.getUri(found, /self/)).not.to.equal(SemanticLink.getUri(resource, /self/));
        });

        it('document with self and title returns item in collection', () => {

            let documentWithTitle = {
                links: [{
                    rel: 'self', href: 'http://example.com/role/1'
                }],
                title: 'Admin'
            };

            let collectionWithTitleDocument = {
                links: [
                    {rel: 'self', href: 'http://example.com/role/'}
                ],
                items: [documentWithTitle]
            };

            let found = _(collectionWithTitleDocument).findResourceInCollection(documentWithTitle);
            expect(found).to.equal(documentWithTitle);
        });

        describe('different attribute name', () => {
            const documentWithUndefinedName = {
                links: [
                    {rel: 'self', href: 'http://localhost:1080/role/49'}
                ],
                name: undefined
            };
            const item = {
                links: [
                    {rel: 'self', href: 'http://localhost:1080/role/49'}
                ],
                name: 'Administrator'
            };
            const collection = {
                links: [{rel: 'self', href: 'http://localhost:1080/tenant/12/role/'}],
                items: [item],
            };

            it('should should return a resource on self when attribute name or link relation is undefined', () => {
                let found = _(collection).findResourceInCollection(documentWithUndefinedName, undefined);
                expect(found).to.equal(item);
            });

            it('should should return a resource on self when mapping overrides link relation matching on name', () => {
                let found = _(collection).findResourceInCollection(documentWithUndefinedName, 'name');
                expect(found).to.equal(item);
            });
        });
    });

    describe('_.findResourceInCollectionByRelAndAttribute', () => {

        let document = {
            links: [{
                rel: 'self', href: 'http://example.com/role/1'
            }],
            name: 'Admin'
        };

        let collection = {
            links: [
                {rel: 'self', href: 'http://example.com/role/'}
            ],
            items: [document]
        };

        it('should return document', () => {
            let found = _(collection).findResourceInCollectionByRelAndAttribute(document, /self/, 'name');
            expect(found).to.deep.equal(document);
        });

        it('should return document with resource identifier as document with defaults', () => {
            let found = _(collection).findResourceInCollectionByRelAndAttribute(document);
            expect(found).to.deep.equal(document);
        });

        it('should not return a document on non-matching attribute', () => {
            let notFound = _(collection).findResourceInCollectionByRelAndAttribute(document, /self/, 'title');
            expect(notFound).to.be.undefined;
        });

        it('should not return a document on non-matching link relation', () => {
            let notFound = _(collection).findResourceInCollectionByRelAndAttribute(document, /not-found/, 'name');
            expect(notFound).to.be.undefined;
        });

        it('should not return a document on non-matching link relation and attribute', () => {
            let notFound = _(collection).findResourceInCollectionByRelAndAttribute(document, /not-found/, 'title');
            expect(notFound).to.be.undefined;
        });

    });

    describe('_.findResourceInCollectionByUri', () => {

        it('document with self and name returns item in collection', () => {
            let found = _(collection).findResourceInCollection(document);
            expect(found).to.deep.equal(document);
        });

        it('document with self and without name returns item in collection', () => {
            let resource = {
                links: [{
                    rel: 'self', href: 'http://example.com/role/1'
                }]
            };

            let found = _(collection).findResourceInCollectionByUri(resource);
            expect(found).to.deep.equal(document);
        });

        it('document without self and with name returns item from collection and not resource as search input', () => {

            let resource = {
                links: [{
                    rel: 'parent', href: 'http://example.com/role/2'
                }],
                name: 'Admin'
            };

            let found = _(collection).findResourceInCollectionByUri(resource, 'parent');
            expect(found).to.be.undefined;
        });
    });

    describe('_.mapUriList', () => {

        it('should convert items', () => {
            const values = [
                {links: [{rel: 'self', href: 'http://localhost:1080/role/50'}],},
                {links: [{rel: 'self', href: 'http://localhost:1080/role/49'}],}
            ];
            expect(_(values).mapUriList()).to.deep.equal(['http://localhost:1080/role/50', 'http://localhost:1080/role/49']);
        });

        it('should not return undefined in the list', () => {
            const values = [
                {links: [{rel: 'self', href: 'http://localhost:1080/role/50'}],},
                {links: [{rel: 'up', href: 'http://localhost:1080/role/49'}],}
            ];
            expect(_(values).mapUriList()).to.deep.equal(['http://localhost:1080/role/50']);
        });

        it('should empty list on empty list', () => {
            const values = [];
            expect(_(values).mapUriList()).to.deep.equal([]);
        });

        it('should empty list on empty list on none found', () => {
            const values = [
                {links: [{rel: 'up', href: 'http://localhost:1080/role/49'}],}
            ];
            expect(_(values).mapUriList()).to.deep.equal([]);
        });

        it('should empty list on empty list on undefined', () => {
            expect(_(undefined).mapUriList()).to.deep.equal([]);
        });

    });

});